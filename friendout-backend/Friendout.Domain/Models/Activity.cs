using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Friendout.Domain.Models
{
    [Table("activities")]
    public class Activity
    {
        [Key]
        [Column("id", TypeName = "varchar(191)")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Column("title", TypeName = "varchar(191)")]
        public string Title { get; set; } = null!;

        [Required]
        [Column("description", TypeName = "text")]
        public string Description { get; set; } = null!;

        [Required]
        [Column("start_at", TypeName = "datetime(3)")]
        public DateTime StartAt { get; set; }

        [Column("end_at", TypeName = "datetime(3)")]
        public DateTime? EndAt { get; set; }

        [ForeignKey(nameof(LocalisationId))]
        public required Localisation Localisation { get; set; }

        [Column("localisation_id", TypeName = "varchar(191)")]
        public string? LocalisationId { get; set; }

        [Column("estimated_price")]
        public double? EstimatedPrice { get; set; }

        [Column("image_id", TypeName = "varchar(191)")]
        public string? ImageId { get; set; }

        [Required]
        [Column("created_by", TypeName = "varchar(191)")]
        public string CreatedBy { get; set; } = null!;

        [Required]
        [Column("created_at", TypeName = "datetime(3)")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("updated_at", TypeName = "datetime(3)")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relations
        [ForeignKey(nameof(CreatedBy))]
        public User Creator { get; set; } = null!;

        [ForeignKey(nameof(ImageId))]
        public Image? Image { get; set; }

        public ICollection<SubActivity> SubActivities { get; set; } = new List<SubActivity>();
        public ICollection<UserEquipment> UserEquipments { get; set; } = [];
        public ICollection<UserParticipation> UserParticipations { get; set; } = new List<UserParticipation>();
        public ICollection<ActivityComment> Comments { get; set; } = new List<ActivityComment>();
        public List<ActivityEquipment>? ActivityEquipments { get; set; } = new List<ActivityEquipment>();
    }
}
