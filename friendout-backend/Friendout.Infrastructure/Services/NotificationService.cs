using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Friendout.Domain.Context;
using Friendout.Domain.Enums;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Friendout.Infrastructure.Services;

/// <summary>
/// Observer pattern — a concrete observer reacting to notification events.
///
/// Receives events from the NotificationDispatcher, resolves user preferences,
/// then delegates to the appropriate INotificationStrategy implementations (email, push...).
///
/// Strategy pattern — strategies are injected as IEnumerable&lt;INotificationStrategy&gt;.
/// Adding a new channel = implement INotificationStrategy + register in DI.
/// This class never needs to change.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IEnumerable<INotificationStrategy> _strategies;
    private readonly FriendoutDbContext _db;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IEnumerable<INotificationStrategy> strategies,
        FriendoutDbContext db,
        ILogger<NotificationService> logger)
    {
        _strategies = strategies;
        _db         = db;
        _logger     = logger;
    }

    public async Task NotifyUserAsync(Guid userId, NotificationType type, Dictionary<string, string> data)
    {
        var (notifPrefs, userPrefs) = await GetUserPreferencesAsync(userId.ToString());

        // Inject Locale into data so strategies never need to resolve it themselves.
        // Caller-supplied Locale is never overwritten — explicit always wins.
        data.TryAdd("Locale", userPrefs.Locale);

        // Filter strategies based on user preferences.
        var activeStrategies = _strategies.Where(s =>
            (s.StrategyName == "Email" && notifPrefs.EmailEnabled) ||
            (s.StrategyName == "InApp"  && notifPrefs.PushEnabled)
        );

        // Execute each active strategy independently — one failure doesn't block the others.
        var tasks = activeStrategies.Select(async strategy =>
        {
            try
            {
                await strategy.SendAsync(userId.ToString(), type, data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send {Type} notification via {Strategy} for user {UserId}",
                    type, strategy.StrategyName, userId);
            }
        });

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Fetches both the notification and general preferences for a user from the DB.
    /// Falls back to sensible defaults if no records exist yet.
    /// </summary>
    private async Task<(UserNotificationPreferences notif, UserPreferences prefs)> GetUserPreferencesAsync(string userId)
    {
        var notifPrefs = await _db.UserNotificationPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId)
            ?? new UserNotificationPreferences
            {
                UserId       = userId,
                EmailEnabled = true,
                PushEnabled  = false
            };

        var userPrefs = await _db.UserPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? new UserPreferences
            {
                UserId = userId,
                Locale = "en"
            };

        return (notifPrefs, userPrefs);
    }
}
