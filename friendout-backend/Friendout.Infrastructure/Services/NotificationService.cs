using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Friendout.Domain.Enums;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace Friendout.Infrastructure.Services;

/// <summary>
/// Observer pattern — a concrete observer reacting to notification events.
///
/// Receives events from the NotificationDispatcher, resolves user preferences,
/// then delegates to the appropriate INotificationStrategy implementations (email, in-app...).
///
/// Strategy pattern — strategies are injected as IEnumerable&lt;INotificationStrategy&gt;.
/// Adding a new channel = implement INotificationStrategy + register in DI.
/// This class never needs to change.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IEnumerable<INotificationStrategy> _strategies;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IEnumerable<INotificationStrategy> strategies, ILogger<NotificationService> logger)
    {
        _strategies = strategies;
        _logger = logger;
    }

    public async Task NotifyUserAsync(Guid userId, NotificationType type, Dictionary<string, string> data)
    {
        // TODO: replace with a real DB lookup once UserNotificationPreference table exists.
        var settings = await GetUserSettingsAsync(userId);

        // Filter strategies based on user preferences.
        var activeStrategies = _strategies.Where(s =>
            (s.StrategyName == "Email" && settings.EmailEnabled) ||
            (s.StrategyName == "InApp"  && settings.PushEnabled)
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
    /// Temporary mock — always returns EmailEnabled = true until the
    /// UserNotificationPreference table and migration are in place.
    /// </summary>
    private Task<NotificationSettings> GetUserSettingsAsync(Guid userId)
    {
        return Task.FromResult(new NotificationSettings
        {
            UserId       = userId,
            EmailEnabled = true,
            PushEnabled  = false
        });
    }
}
