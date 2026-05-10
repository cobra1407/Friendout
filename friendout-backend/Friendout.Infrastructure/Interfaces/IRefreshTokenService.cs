using System.Threading.Tasks;
using Friendout.Domain.Models;

namespace Friendout.Infrastructure.Interfaces;

/// <summary>
/// Manages creation, validation, and revocation of refresh tokens.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>
    /// Creates a new refresh token for the given user, persists it to the database,
    /// and returns the raw token value to be sent to the client as a cookie.
    /// </summary>
    Task<string> CreateAsync(string userId);

    /// <summary>
    /// Validates the given raw token value.
    /// Returns the RefreshToken entity if valid, null otherwise.
    /// A token is invalid if it does not exist, is expired, or has been revoked.
    /// </summary>
    Task<RefreshToken?> ValidateAsync(string token);

    /// <summary>
    /// Revokes a token by marking it as revoked in the database.
    /// Called on logout or when a rotation anomaly is detected.
    /// </summary>
    Task RevokeAsync(string token);

    /// <summary>
    /// Rotates a refresh token: revokes the old one and creates a new one for the same user.
    /// This limits the damage if a refresh token is stolen — each token can only be used once.
    /// </summary>
    Task<string> RotateAsync(string oldToken, string userId);

    /// <summary>
    /// Revokes all active refresh tokens for a given user.
    /// Called on login to ensure only one active session exists per user.
    /// </summary>
    Task RevokeAllAsync(string userId);
}
