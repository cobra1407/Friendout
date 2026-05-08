using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Friendout.Domain.Models;

public enum AccessRequestStatus
{
    Pending,
    Approved,
    Denied
}

/// <summary>
/// A request from a user who wants access to Friendout but is not yet authorized.
/// The admin can approve or deny it from the admin panel.
/// </summary>
[Table("access_requests")]
public class AccessRequest
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("email")]
    [MaxLength(191)]
    public string Email { get; set; } = null!;

    [Column("name")]
    [MaxLength(191)]
    public string? Name { get; set; }

    /// <summary>Optional message from the requester explaining why they want access.</summary>
    [Column("message")]
    [MaxLength(500)]
    public string? Message { get; set; }

    [Column("status")]
    public AccessRequestStatus Status { get; set; } = AccessRequestStatus.Pending;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("resolved_at")]
    public DateTime? ResolvedAt { get; set; }
}
