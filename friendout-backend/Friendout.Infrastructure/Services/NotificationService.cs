using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Friendout.Domain.Context;
using Friendout.Domain.Enums;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Friendout.Infrastructure.Services;

/// <summary>
/// Observer pattern — a concrete observer reacting to notification events.
///
/// Receives events from the NotificationDispatcher, resolves user preferences,
/// then delegates to the appropriate INotificationStrategy implementations (email, push...).
///
/// Uses IServiceScopeFactory to create a fresh DI scope per notification, avoiding
/// ObjectDisposedException when running fire-and-forget after the HTTP request ends.
///
/// Strategy pattern — strategies are injected as IEnumerable&lt;INotificationStrategy&gt;.
/// Adding a new channel = implement INotificationStrategy + register in DI.
/// This class never needs to change.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IEnumerable<INotificationStrategy> _strategies;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IEnumerable<INotificationStrategy> strategies,
        IServiceScopeFactory scopeFactory,
        ILogger<NotificationService> logger)
    {
        _strategies   = strategies;
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    public async Task NotifyUserAsync(Guid userId, NotificationType type, Dictionary<string, string> data)
    {
        // Create a fresh scope so the DbContext is never disposed under us,
        // even when this runs fire-and-forget after the HTTP request ends.
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db         = scope.ServiceProvider.GetRequiredService<FriendoutDbContext>();
        var appLog     = scope.ServiceProvider.GetRequiredService<IAppLogService>();

        var (notifPrefs, userPrefs) = await GetUserPreferencesAsync(db, userId.ToString());

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

                // Also log to the admin panel so admins can diagnose delivery issues
                // (e.g. misconfigured SMTP, missing credentials).
                await appLog.LogErrorAsync(
                    "Notifications",
                    $"Failed to send {type} notification via {strategy.StrategyName} for user {userId}: {ex.Message}",
                    ex);
            }
        });

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Fetches both the notification and general preferences for a user from the DB.
    /// Falls back to sensible defaults if no records exist yet.
    /// </summary>
    private static async Task<(UserNotificationPreferences notif, UserPreferences prefs)> GetUserPreferencesAsync(
        FriendoutDbContext db, string userId)
    {
        var notifPrefs = await db.UserNotificationPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId)
            ?? new UserNotificationPreferences
            {
                UserId       = userId,
                EmailEnabled = true,
                PushEnabled  = false
            };

        var userPrefs = await db.UserPreferences
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
