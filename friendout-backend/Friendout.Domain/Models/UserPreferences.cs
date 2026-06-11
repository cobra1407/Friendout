using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Friendout.Domain.Models;

/// <summary>
/// General user preferences (display, language, etc.).
/// One-to-one with User — created on first save with sensible defaults.
/// </summary>
[Table("user_preferences")]
public class UserPreferences
{
    [Key]
    [Column("user_id")]
    [MaxLength(191)]
    public string UserId { get; set; } = null!;

    /// <summary>
    /// The user's preferred locale for UI and emails (e.g. "fr", "en").
    /// Defaults to "en".
    /// </summary>
    [Column("locale")]
    [MaxLength(10)]
    public string Locale { get; set; } = "en";

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
}
