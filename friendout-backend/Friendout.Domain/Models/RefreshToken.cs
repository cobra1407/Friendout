namespace Friendout.Domain.Models;

/// <summary>
/// Represents a refresh token issued to a user after login.
///
/// Refresh tokens allow obtaining a new short-lived access token (JWT) without
/// requiring the user to log in again. Each token is single-use: once consumed,
/// it is replaced by a new one (token rotation).
/// </summary>
public class RefreshToken
{
    /// <summary>Primary key — the raw token value stored as a hash in practice.</summary>
    public string Token { get; set; } = null!;

    /// <summary>The user this token belongs to.</summary>
    public string UserId { get; set; } = null!;
    public User User { get; set; } = null!;

    /// <summary>When the token was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>When the token expires (30 days after creation).</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Whether the token has been revoked (e.g. on logout or suspicious activity).</summary>
    public bool IsRevoked { get; set; } = false;
}
