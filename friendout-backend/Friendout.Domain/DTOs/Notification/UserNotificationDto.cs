using Friendout.Domain.Enums;

namespace Friendout.Domain.DTOs.Notification;

public class UserNotificationDto
{
    public int Id { get; set; }
    public NotificationType Type { get; set; }

    /// <summary>
    /// JSON-serialized dictionary of template variables.
    /// The frontend uses these to render the notification in the user's locale.
    /// </summary>
    public string Payload { get; set; } = "{}";

    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
