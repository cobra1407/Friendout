using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Friendout.Domain.Models
{
    [Table("activity_equipment")]
    public class ActivityEquipment
    {
        [Key]
        [Column("id", TypeName = "varchar(191)")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Column("activityId", TypeName = "varchar(191)")]
        public string ActivityId { get; set; } = null!;

        [Required]
        [Column("equipmentId", TypeName = "varchar(191)")]
        public string EquipmentId { get; set; } = null!;

        [Required]
        [Column("required")]
        public bool Required { get; set; } = false;

        [Required]
        [Column("quantity")]
        public int Quantity { get; set; } = 1;

        [Required]
        [Column("createdAt", TypeName = "datetime(3)")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("updatedAt", TypeName = "datetime(3)")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relations
        [ForeignKey(nameof(ActivityId))]
        public Activity Activity { get; set; } = null!;

        [ForeignKey(nameof(EquipmentId))]
        public Equipment Equipment { get; set; } = null!;
    }
}