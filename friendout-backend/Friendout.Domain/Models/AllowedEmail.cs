using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Friendout.Domain.Models;

/// <summary>
/// A Gmail address explicitly allowed to log in via Google OAuth.
/// Only used once Google OAuth is implemented.
/// </summary>
[Table("allowed_emails")]
public class AllowedEmail
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("email")]
    [MaxLength(191)]
    public string Email { get; set; } = null!;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
