using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Friendout.Domain.Enums;

namespace Friendout.Domain.Models
{
    [Table("localisation")]
    public class Localisation
    {
        [Key]
        [Column("id", TypeName = "varchar(191)")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Column("type", TypeName = "varchar(50)")]
        public LocalisationType Type { get; set; }       // "address", "maplink", "virtual"

        [Column("address", TypeName = "varchar(500)")]
        public string? Address { get; set; }

        [Column("mapLink", TypeName = "varchar(500)")]
        public string? MapLink { get; set; }

        [Column("virtualUrl", TypeName = "varchar(500)")]
        public string? VirtualUrl { get; set; }

        [Column("displayName", TypeName = "varchar(100)")]
        public string? DisplayName { get; set; }
        

        // Relation inverse
        public ICollection<Activity> Activities { get; set; } = new List<Activity>();
    }
}