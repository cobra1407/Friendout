using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Friendout.Domain.Enums;

namespace Friendout.Domain.Models;

/// <summary>
/// Represents an in-app notification for a user.
/// Created by InAppNotificationStrategy when a notification is dispatched.
///
/// Instead of storing pre-rendered text, we store the raw payload (JSON dict of
/// template variables). The frontend resolves the human-readable title/message
/// at render time using the user's current locale, so language changes and copy
/// edits are reflected on all past notifications automatically.
/// </summary>
[Table("user_notifications")]
public class UserNotification
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("user_id")]
    [MaxLength(191)]
    public string UserId { get; set; } = null!;

    [Required]
    [Column("type")]
    public NotificationType Type { get; set; }

    /// <summary>
    /// JSON-serialized dictionary of template variables (e.g. ActivityName, OrganizerName).
    /// The frontend uses these to render the notification in the user's current locale.
    /// </summary>
    [Required]
    [Column("payload", TypeName = "json")]
    public string Payload { get; set; } = "{}";

    [Column("is_read")]
    public bool IsRead { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
}
