using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Friendout.Domain.Models
{
    [Table("activity_equipments")]
    public class ActivityEquipment
    {
        [Key]
        [Column("id", TypeName = "varchar(191)")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Column("activity_id", TypeName = "varchar(191)")]
        public string ActivityId { get; set; } = null!;

        [Required]
        [Column("equipment_id", TypeName = "varchar(191)")]
        public string EquipmentId { get; set; } = null!;

        [Required]
        [Column("required")]
        public bool Required { get; set; } = false;

        [Required]
        [Column("quantity")]
        public int Quantity { get; set; } = 1;

        [Required]
        [Column("created_at", TypeName = "datetime(3)")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("updated_at", TypeName = "datetime(3)")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relations
        [ForeignKey(nameof(ActivityId))]
        public Activity Activity { get; set; } = null!;

        [ForeignKey(nameof(EquipmentId))]
        public Equipment Equipment { get; set; } = null!;
    }
}