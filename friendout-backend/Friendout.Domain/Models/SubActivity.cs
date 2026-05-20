using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Friendout.Domain.Models
{
    [Table("sub_activities")]
    public class SubActivity
    {
        [Key]
        [Column("id")]
        [MaxLength(191)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Column("name")]
        [MaxLength(191)]
        public string Name { get; set; } = null!;
        
        [ForeignKey(nameof(LocalisationId))]
        public required Localisation Localisation { get; set; }
        
        [Column("localisation_id", TypeName = "varchar(191)")]
        public string? LocalisationId { get; set; }

        [Required]
        [Column("start_time", TypeName = "datetime(3)")]
        public DateTime StartTime { get; set; }

        [Required]
        [Column("end_time", TypeName = "datetime(3)")]
        public DateTime EndTime { get; set; }

        [Column("description", TypeName = "text")]
        public string? Description { get; set; }

        [Column("price")]
        public double? Price { get; set; }

        [Required]
        [Column("activity_id")]
        [MaxLength(191)]
        public string ActivityId { get; set; } = null!;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relations
        [ForeignKey("ActivityId")]
        public Activity Activity { get; set; } = null!;

        public ICollection<UserParticipation> UserParticipations { get; set; }
            = new List<UserParticipation>();
    }
}
    