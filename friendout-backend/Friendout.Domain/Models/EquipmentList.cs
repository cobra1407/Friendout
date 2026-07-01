using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
