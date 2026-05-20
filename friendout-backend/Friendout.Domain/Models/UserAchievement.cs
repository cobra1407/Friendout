using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Friendout.Domain.Models
{
    [Table("user_achievements")]
    public class UserAchievement
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        [MaxLength(191)]
        public string UserId { get; set; } = null!;

        [Required]
        [Column("achievement_id")]
        public int AchievementId { get; set; }

        [Column("unlocked_at")]
        public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;

        // Relations
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [ForeignKey("AchievementId")]
        public Achievement Achievement { get; set; } = null!;
    }
}
