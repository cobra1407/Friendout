using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Friendout.Domain.Enums;

namespace Friendout.Domain.Models;

/// <summary>
/// An application-level log entry persisted in the database.
/// Only meaningful events are stored here (auth, admin actions, errors).
/// Messages are plain English strings — always human-readable regardless of i18n.
/// Low-level/debug logs stay in the console (stdout → Docker).
/// </summary>
[Table("app_logs")]
public class AppLog
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>Severity level: Info, Warning or Error.</summary>
    [Column("level")]
    public AppLogLevel Level { get; set; }

    /// <summary>Source area, e.g. "Auth", "Admin", "System".</summary>
    [Required]
    [Column("category")]
    [MaxLength(100)]
    public string Category { get; set; } = null!;

    /// <summary>Human-readable English description of the event.</summary>
    [Required]
    [Column("message")]
    [MaxLength(500)]
    public string Message { get; set; } = null!;

    /// <summary>Full exception string (only for Error level).</summary>
    [Column("exception", TypeName = "TEXT")]
    public string? Exception { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
