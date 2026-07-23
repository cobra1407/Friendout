using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Friendout.Domain.Enums;
using Friendout.Infrastructure.Interfaces;

namespace Friendout.Infrastructure.Services;

/// <summary>
/// Strategy pattern — live push delivery channel, complementing InAppNotificationStrategy.
///
/// Doesn't persist anything (that's InApp's job) — just pushes the same event over the
/// ActivitiesHub so a connected client updates its notification bell instantly instead of
/// waiting for the next poll. If the push fails or the user isn't connected, nothing is lost:
/// the InApp strategy already persisted the notification, so it'll show up on next load/poll
/// regardless.
///
/// Shares the InAppEnabled preference (see NotificationService's active-strategy filter) rather
/// than having its own toggle — this is purely a delivery-speed enhancement of "in-app", not a
/// separate channel a user would want to configure independently.
/// </summary>
public class WebSocketNotificationStrategy : INotificationStrategy
{
    private readonly IActivitiesHubNotifier _hubNotifier;

    public string StrategyName => "WebSocket";

    public WebSocketNotificationStrategy(IActivitiesHubNotifier hubNotifier)
    {
        _hubNotifier = hubNotifier;
    }

    public Task SendAsync(string userId, NotificationType type, Dictionary<string, string> data)
    {
        // Guid.Empty means the recipient has no account yet (e.g. access request emails) —
        // nothing to push to, mirrors InAppNotificationStrategy's same guard.
        if (!Guid.TryParse(userId, out var parsed) || parsed == Guid.Empty)
            return Task.CompletedTask;

        return _hubNotifier.NotifyUserAsync(userId, type, data);
    }
}
