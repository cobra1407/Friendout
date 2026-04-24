using System;
using System.Collections.Concurrent;
using Friendout.Infrastructure.Interfaces;

namespace Friendout.Infrastructure.Services;

/// <summary>
/// In-memory implementation of the JWT blacklist.
///
/// How it works:
/// - A thread-safe dictionary maps each invalidated Jti to its expiry date.
/// - On every check, expired entries are pruned so memory doesn't grow indefinitely.
/// - If the server restarts, the blacklist is lost — but tokens expire naturally
///   within 7 days, which is acceptable for a single-instance self-hosted app.
/// </summary>
public class TokenBlacklistService : ITokenBlacklistService
{
    // ConcurrentDictionary is thread-safe — multiple requests can read/write simultaneously.
    private readonly ConcurrentDictionary<string, DateTime> _blacklist = new();

    /// <inheritdoc />
    public void Invalidate(string jti, DateTime expiry)
    {
        _blacklist[jti] = expiry;
        PruneExpired();
    }

    /// <inheritdoc />
    public bool IsBlacklisted(string jti)
    {
        if (!_blacklist.TryGetValue(jti, out var expiry))
            return false;

        // If the token has naturally expired, remove it and treat it as not blacklisted
        // (the JWT middleware will reject it anyway due to expiry).
        if (expiry < DateTime.UtcNow)
        {
            _blacklist.TryRemove(jti, out _);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Removes all entries whose tokens have already expired.
    /// Called on each Invalidate() to keep memory usage bounded.
    /// </summary>
    private void PruneExpired()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in _blacklist)
        {
            if (entry.Value < now)
                _blacklist.TryRemove(entry.Key, out _);
        }
    }
}
