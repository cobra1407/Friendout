using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Friendout.Domain.Models
{
    /// <summary>
    /// Item of an <see cref="EquipmentList"/>. Stores only the equipment name,
    /// matching the format used by the EquipmentManager component on the frontend.
    /// </summary>
    [Table("equipment_list_items")]
    public class EquipmentListItem
    {
        [Key]
        [Column("id", TypeName = "varchar(191)")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Column("equipment_list_id", TypeName = "varchar(191)")]
        public string EquipmentListId { get; set; } = null!;

        [Required]
        [Column("name", TypeName = "varchar(191)")]
        public string Name { get; set; } = null!;

        // Relations
        [ForeignKey(nameof(EquipmentListId))]
        public EquipmentList EquipmentList { get; set; } = null!;
    }
}
