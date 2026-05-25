namespace Friendout.Domain.Models;

/// <summary>
/// Represents the notification preferences for a user.
/// </summary>
public class NotificationSettings
{
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets whether the user wants to receive email notifications.
    /// </summary>
    public bool EmailEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the user wants to receive push notifications.
    /// </summary>
    public bool PushEnabled { get; set; } = false;
}
