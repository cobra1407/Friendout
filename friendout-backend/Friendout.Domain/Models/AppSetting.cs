using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Friendout.Domain.Models;

/// <summary>
/// A generic key-value store for application-wide settings managed by admins.
/// </summary>
[Table("app_settings")]
public class AppSetting
{
    [Key]
    [Column("key")]
    [MaxLength(100)]
    public string Key { get; set; } = null!;

    [Required]
    [Column("value")]
    [MaxLength(500)]
    public string Value { get; set; } = null!;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
