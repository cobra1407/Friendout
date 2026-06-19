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

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
}
