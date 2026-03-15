using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Friendout.Domain.Models
{
    [Table("Activity_comment")]
    public class ActivityComment
    {
        [Key]
        [Column("id", TypeName = "varchar(191)")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Column("activityId", TypeName = "varchar(191)")]
        public string ActivityId { get; set; } = null!;

        [Required]
        [Column("userId", TypeName = "varchar(191)")]
        public string UserId { get; set; } = null!;

        [Required]
        [Column("content", TypeName = "text")]
        public string Content { get; set; } = null!;

        [Required]
        [Column("createdAt", TypeName = "datetime(3)")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("updatedAt", TypeName = "datetime(3)")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relations
        [ForeignKey(nameof(ActivityId))]
        public Activity Activity { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;
    }
}