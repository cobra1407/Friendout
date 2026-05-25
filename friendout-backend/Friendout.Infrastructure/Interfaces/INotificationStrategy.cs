using System.Collections.Generic;
using System.Threading.Tasks;
using Friendout.Domain.Enums;

namespace Friendout.Infrastructure.Interfaces;

/// <summary>
/// Strategy pattern — common contract for all notification delivery channels.
///
/// Each channel (Email, InApp, Push...) implements this interface independently.
/// The dispatcher only depends on this interface, never on concrete classes.
/// Adding a new channel = implement this interface + register in DI. Nothing else changes.
/// </summary>
public interface INotificationStrategy
{
    /// <summary>
    /// Unique name used to match against user preferences (e.g. "Email", "InApp").
    /// Must be stable — changing it breaks existing preference records.
    /// </summary>
    string StrategyName { get; }

    /// <summary>
    /// Sends a notification to the given user.
    /// </summary>
    /// <param name="userId">The recipient's user ID.</param>
    /// <param name="type">Determines which template to use.</param>
    /// <param name="data">Variables injected into the template (e.g. {{ UserEmail }}).</param>
    Task SendAsync(string userId, NotificationType type, Dictionary<string, string> data);
}
