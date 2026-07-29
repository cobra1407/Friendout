using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Friendout.Domain.Models;

/// <summary>
/// Notification channel preferences for a user.
/// One-to-one with User — created on first save with sensible defaults.
/// </summary>
[Table("user_notification_preferences")]
public class UserNotificationPreferences
{
    [Key]
    [Column("user_id")]
    [MaxLength(191)]
    public string UserId { get; set; } = null!;

    /// <summary>Whether the user wants to receive email notifications.</summary>
    [Column("email_enabled")]
    public bool EmailEnabled { get; set; } = true;

    /// <summary>Whether the user wants to receive in-app notifications.</summary>
    [Column("in_app_enabled")]
    public bool InAppEnabled { get; set; } = true;

    /// <summary>
    /// Which sound to play when a live in-app notification arrives (see the frontend's sound
    /// catalog for the actual list — this is just an opaque key, e.g. "default", "chime").
    /// Not validated against a fixed server-side list on purpose: the catalog lives entirely
    /// in the frontend and can grow without a backend change. If a stored key no longer exists
    /// in the catalog (a sound was removed), the frontend falls back to its own default.
    /// </summary>
    [Column("notification_sound")]
    [MaxLength(50)]
    public string NotificationSound { get; set; } = "default";

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
}
