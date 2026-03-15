using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Friendout.Domain.Models
{
    [Table("equipment")]
    public class Equipment
    {
        [Key]
        [Column("id", TypeName = "varchar(191)")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Column("name", TypeName = "varchar(191)")]
        public string Name { get; set; } = null!;

        [Column("description", TypeName = "text")]
        public string? Description { get; set; }
        
        
        // Relations
        public ICollection<UserEquipment> UserEquipments { get; set; } = new List<UserEquipment>();
        public ICollection<ActivityEquipment> ActivityEquipments { get; set; } = new List<ActivityEquipment>();
    }
}