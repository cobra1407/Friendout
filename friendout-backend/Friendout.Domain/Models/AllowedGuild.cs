using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Friendout.Domain.Models;

/// <summary>
/// A Discord guild (server) whose members are allowed to access Friendout.
/// Stored in the database so admins can manage them via the admin panel
/// without restarting the server.
/// </summary>
[Table("allowed_guilds")]
public class AllowedGuild
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>The Discord guild (server) ID.</summary>
    [Required]
    [Column("guild_id")]
    [MaxLength(50)]
    public string GuildId { get; set; } = null!;

    /// <summary>Human-readable label set by the admin (e.g. "Main server").</summary>
    [Column("label")]
    [MaxLength(100)]
    public string? Label { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
