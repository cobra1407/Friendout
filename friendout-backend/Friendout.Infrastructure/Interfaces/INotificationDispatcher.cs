using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Friendout.Domain.Enums;

namespace Friendout.Infrastructure.Interfaces;

/// <summary>
/// Observer pattern — the subject that publishes notification events.
///
/// Callers (e.g. AdminService) fire a notification event without knowing
/// which channels will handle it. The dispatcher forwards the event to all
/// registered INotificationService observers.
/// </summary>
public interface INotificationDispatcher
{
    /// <summary>
    /// Dispatches a notification event to all registered observers.
    /// </summary>
    /// <param name="userId">The recipient's user ID.</param>
    /// <param name="type">The notification type — determines which template to use.</param>
    /// <param name="data">Template variables (e.g. { "UserEmail", "thomas@gmail.com" }).</param>
    Task DispatchNotificationAsync(Guid userId, NotificationType type, Dictionary<string, string> data);
}
