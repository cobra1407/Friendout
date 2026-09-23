using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Friendout.Domain.Models;

/// <summary>
/// A short-lived, single-use token allowing a user to confirm deletion of their
/// personal data via a link sent to their email address.
///
/// Exists for users who can no longer authenticate (e.g. their Discord guild access
/// was revoked) but still need a way to exercise their right to erasure without
/// going through the normal [Authorize] endpoints.
/// </summary>
[Table("account_deletion_requests")]
public class AccountDeletionRequest
{
    /// <summary>Primary key — the raw token value sent in the confirmation email.</summary>
    [Key]
    [Column("token")]
    [MaxLength(191)]
    public string Token { get; set; } = null!;

    /// <summary>The user requesting deletion of their personal data.</summary>
    [Required]
    [Column("user_id")]
    [MaxLength(191)]
    public string UserId { get; set; } = null!;
    public User User { get; set; } = null!;

    /// <summary>When the request was created.</summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>When the token expires (1 hour after creation).</summary>
    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Whether the user chose to also delete every activity they created, even ones
    /// other people are participating in — captured at request time since the confirm
    /// step only receives the token, not the original form data. Defaults to false
    /// (the safer choice: orphan shared activities rather than delete them).
    /// </summary>
    [Column("delete_created_activities")]
    public bool DeleteCreatedActivities { get; set; }
}
