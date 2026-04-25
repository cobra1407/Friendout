using System;

namespace Friendout.Infrastructure.Interfaces;

/// <summary>
/// Manages a blacklist of invalidated JWT tokens.
/// When a user logs out, their token's unique ID (Jti) is added here.
/// The JWT middleware checks this list on every request.
/// </summary>
public interface ITokenBlacklistService
{
    /// <summary>
    /// Adds a token's Jti to the blacklist so it can no longer be used.
    /// </summary>
    /// <param name="jti">The unique identifier of the JWT (JwtRegisteredClaimNames.Jti).</param>
    /// <param name="expiry">When the token naturally expires — used to auto-clean the blacklist.</param>
    void Invalidate(string jti, DateTime expiry);

    /// <summary>
    /// Returns true if the given Jti has been blacklisted (i.e. the user logged out).
    /// </summary>
    bool IsBlacklisted(string jti);
}
