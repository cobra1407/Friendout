using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Friendout.Domain.Models
{
    [Table("verification_tokens")]
    public class VerificationToken
    {
        [Key]
        [Column("token")]
        [MaxLength(255)]
        public string Token { get; set; } = null!;

        [Required]
        [Column("identifier")]
        [MaxLength(191)]
        public string Identifier { get; set; } = null!;

        [Required]
        [Column("expires")]
        public DateTime Expires { get; set; }
    }
}
