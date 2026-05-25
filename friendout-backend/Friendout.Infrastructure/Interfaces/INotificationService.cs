using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Friendout.Domain.Enums;

namespace Friendout.Infrastructure.Interfaces;

/// <summary>
/// Observer pattern — an observer that reacts to notification events.
///
/// Receives events from the dispatcher, checks user preferences,
/// and delegates to the appropriate INotificationStrategy implementations.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a notification to a user using their configured strategies.
    /// </summary>
    /// <param name="userId">The recipient's user ID.</param>
    /// <param name="type">The notification type — determines which template to use.</param>
    /// <param name="data">Template variables (e.g. { "UserEmail", "thomas@gmail.com" }).</param>
    Task NotifyUserAsync(Guid userId, NotificationType type, Dictionary<string, string> data);
}
