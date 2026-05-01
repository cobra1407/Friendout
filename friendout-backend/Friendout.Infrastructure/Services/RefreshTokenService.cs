using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Friendout.Domain.Context;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Friendout.Infrastructure.Services;

/// <summary>
/// Handles all refresh token operations: creation, validation, rotation, and revocation.
///
/// Security design:
/// - Tokens are 256-bit random values (cryptographically secure).
/// - Each token is single-use: consuming it immediately creates a new one (rotation).
/// - Expired and revoked tokens are kept in the database for audit purposes
///   but are rejected during validation.
/// - If a revoked token is presented (replay attack), all tokens for that user
///   should ideally be revoked. This is a future improvement.
/// </summary>
public class RefreshTokenService : IRefreshTokenService
{
    private readonly FriendoutDbContext _db;
    private const int TokenLifetimeDays = 30;

    public RefreshTokenService(FriendoutDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<string> CreateAsync(string userId)
    {
        var rawToken = GenerateSecureToken();

        var refreshToken = new RefreshToken
        {
            Token     = rawToken,
            UserId    = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(TokenLifetimeDays),
            IsRevoked = false,
        };

        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync();

        return rawToken;
    }

    /// <inheritdoc />
    public async Task<RefreshToken?> ValidateAsync(string token)
    {
        var refreshToken = await _db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token);

        if (refreshToken is null)       return null; // token not found
        if (refreshToken.IsRevoked)     return null; // already used or explicitly revoked
        if (refreshToken.ExpiresAt < DateTime.UtcNow) return null; // expired

        return refreshToken;
    }

    /// <inheritdoc />
    public async Task RevokeAsync(string token)
    {
        var refreshToken = await _db.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == token);

        if (refreshToken is null) return;

        refreshToken.IsRevoked = true;
        await _db.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<string> RotateAsync(string oldToken, string userId)
    {
        // Revoke the old token first — it must never be usable again.
        await RevokeAsync(oldToken);

        // Issue a fresh token for the same user.
        return await CreateAsync(userId);
    }

    /// <summary>
    /// Generates a cryptographically secure random token (256 bits, URL-safe base64).
    /// </summary>
    private static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32); // 256 bits
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
