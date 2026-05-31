using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Friendout.Domain.Models
{
    [Table("user_equipment")]
    public class UserEquipment
    {
        [Required]
        [Column("user_id")]
        [MaxLength(191)]
        public string UserId { get; set; } = null!;

        [Required]
        [Column("equipment_id")]
        [MaxLength(191)]
        public string EquipmentId { get; set; } = null!;

        [Required]
        [Column("activity_id", TypeName = "varchar(191)")]
        public string ActivityId { get; set; } = null!;

        [Required]
        [Column("quantity")]
        public int Quantity { get; set; } = 1;

        // Navigation properties
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [ForeignKey("EquipmentId")]
        public Equipment Equipment { get; set; } = null!;

        [ForeignKey("ActivityId")]
        public Activity Activity { get; set; } = null!;
    }
}
