using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Friendout.Domain.Enums;

namespace Friendout.Domain.Models
{
    [Table("user_participations")]
    public class UserParticipation
    {
        [Key]
        [Column("id")]
        [MaxLength(191)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Column("activity_id")]
        [MaxLength(191)]
        public string ActivityId { get; set; } = null!;

        [Column("sub_activity_id")]
        [MaxLength(191)]
        public string? SubActivityId { get; set; }

        [Required]
        [Column("user_id")]
        [MaxLength(191)]
        public string UserId { get; set; } = null!;

        [Required]
        [Column("status")]
        public ParticipationStatus Status { get; set; }

        // Relations
        [ForeignKey("ActivityId")]
        public Activity Activity { get; set; } = null!;

        [ForeignKey("SubActivityId")]
        public SubActivity? SubActivity { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
