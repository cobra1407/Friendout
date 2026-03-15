using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Friendout.Domain.Models
{
    [Table("user_equipment")]
    public class UserEquipment
    {
        [Required]
        [Column("userId")]
        [MaxLength(191)]
        public string UserId { get; set; } = null!;

        [Required]
        [Column("equipmentId")]
        [MaxLength(191)]
        public string EquipmentId { get; set; } = null!;
        
        [Required]
        [Column("activity_id", TypeName = "varchar(191)")]
        public string ActivityId { get; set; } = null!;

        [ForeignKey("ActivityId")]
        
        [Required]
        [Column("quantity")]
        public int Quantity { get; set; } = 1;

        // Relations
        public User User { get; set; } = null!;
        public Equipment Equipment { get; set; } = null!;
    }

}
