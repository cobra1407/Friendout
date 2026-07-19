using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Friendout.Domain.Constants;

namespace Friendout.Domain.Models
{
    /// <summary>
    /// Reusable named list of equipment created by a user.
    /// Used to prefill the required-equipment field when creating an activity (frontend-only usage).
    /// </summary>
    [Table("equipment_lists")]
    public class EquipmentList
    {
        [Key]
        [Column("id", TypeName = "varchar(191)")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Column("user_id", TypeName = "varchar(191)")]
        public string UserId { get; set; } = null!;

        [Required]
        [Column("name", TypeName = "varchar(191)")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Key identifying the icon to display for this list (e.g. "backpack", "tent").
        /// Must match one of the keys accepted by EquipmentListIcons on the backend
        /// and mapped to a Lucide icon component on the frontend.
        /// </summary>
        [Required]
        [Column("icon", TypeName = "varchar(50)")]
        public string Icon { get; set; } = EquipmentListIcons.Default;

        [Required]
        [Column("created_at", TypeName = "datetime(3)")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("updated_at", TypeName = "datetime(3)")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relations
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        public ICollection<EquipmentListItem> Items { get; set; } = new List<EquipmentListItem>();
    }
}
