using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Friendout.Domain.Models
{
    [Table("achievements")]
    public class Achievement
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("code")]
        [MaxLength(191)]
        public string Code { get; set; } = null!;

        [Required]
        [Column("name")]
        [MaxLength(191)]
        public string Name { get; set; } = null!;

        [Column("icon")]
        [MaxLength(191)]
        public string? Icon { get; set; }

        [Required]
        [Column("description", TypeName = "text")]
        [MaxLength(500)]
        public string Description { get; set; } = null!;

        // Relations
        public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
    }
}
