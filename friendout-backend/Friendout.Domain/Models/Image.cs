using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Friendout.Domain.Models
{
    [Table("images")]
    public class Image
    {
        [Key]
        [Column("id", TypeName = "varchar(191)")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Column("url", TypeName = "text")]
        public string Url { get; set; } = null!;

        [Required]
        [Column("name", TypeName = "varchar(191)")]
        public string Name { get; set; } = null!;

        [Column("description", TypeName = "text")]
        public string? Description { get; set; }

        [Column("mimeType", TypeName = "varchar(191)")]
        public string? MimeType { get; set; }

        [Column("size")]
        public long? Size { get; set; }

        [Column("width")]
        public int? Width { get; set; }

        [Column("height")]
        public int? Height { get; set; }

        [Column("altText", TypeName = "text")]
        public string? AltText { get; set; }

        [Required]
        [Column("createdBy", TypeName = "varchar(191)")]
        public string CreatedBy { get; set; } = null!;
    }
}