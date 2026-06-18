using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Friendout.Domain.Context;
using Friendout.Domain.Enums;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Friendout.Infrastructure.Services;

/// <summary>
/// Strategy pattern — in-app notification delivery strategy.
/// Persists a UserNotification record in the database so the user
/// can see it in the notification bell dropdown.
///
/// The payload (template variables) is stored as JSON instead of pre-rendered text,
/// so the frontend can translate and format the notification in the user's current locale.
/// </summary>
public class InAppNotificationStrategy : INotificationStrategy
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<InAppNotificationStrategy> _logger;

    public string StrategyName => "InApp";

    public InAppNotificationStrategy(
        IServiceScopeFactory scopeFactory,
        ILogger<InAppNotificationStrategy> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    public async Task SendAsync(string userId, NotificationType type, Dictionary<string, string> data)
    {
        // Guid.Empty means the recipient has no account yet (e.g. access request emails).
        // No account = no in-app notification to persist.
        if (!Guid.TryParse(userId, out var parsed) || parsed == Guid.Empty)
            return;

        await using var scope = _scopeFactory.CreateAsyncScope();
        var db                = scope.ServiceProvider.GetRequiredService<FriendoutDbContext>();
        var appLog            = scope.ServiceProvider.GetRequiredService<IAppLogService>();

        try
        {
            // Serialize the full data dict as the payload.
            // The frontend resolves title/message from type + payload at render time.
            var payload = JsonSerializer.Serialize(data);

            db.UserNotifications.Add(new UserNotification
            {
                UserId    = userId,
                Type      = type,
                Payload   = payload,
                IsRead    = false,
                CreatedAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist in-app notification for user {UserId}", userId);
            await appLog.LogErrorAsync("Notifications",
                $"Failed to persist in-app notification ({type}) for user {userId}: {ex.Message}", ex);
        }
    }
}
