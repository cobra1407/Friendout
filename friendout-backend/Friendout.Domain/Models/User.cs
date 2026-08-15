using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Friendout.Domain.Enums;

namespace Friendout.Domain.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        [MaxLength(191)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Column("name")]
        [MaxLength(191)]
        public string Name { get; set; } = null!;

        [Column("email")]
        [MaxLength(191)]
        public string? Email { get; set; }

        [Required]
        [Column("role")]
        public UserRole Role { get; set; } = UserRole.User;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("avatar_url")]
        [MaxLength(255)]
        public string? AvatarUrl { get; set; } = null;

        // Relations
        public ICollection<Account> Accounts { get; set; } = new List<Account>();
        public ICollection<Activity> CreatedActivities { get; set; } = new List<Activity>();
        public ICollection<UserParticipation> UserParticipation { get; set; } = new List<UserParticipation>();
        public ICollection<ActivityComment> Comments { get; set; } = new List<ActivityComment>();
        public ICollection<UserEquipment> UserEquipments { get; set; } = new List<UserEquipment>();
        public ICollection<EquipmentList> EquipmentLists { get; set; } = new List<EquipmentList>();
        public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
        public ICollection<UserNotification> Notifications { get; set; } = new List<UserNotification>();
        public UserPreferences? Preferences { get; set; }
        public UserNotificationPreferences? NotificationPreferences { get; set; }
    }
}
