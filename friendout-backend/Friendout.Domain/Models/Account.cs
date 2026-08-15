using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Friendout.Domain.Models
{
    [Table("accounts")]
    public class Account
    {
        [Key]
        [Column("id")]
        [MaxLength(191)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Column("user_id")]
        [MaxLength(191)]
        public string UserId { get; set; } = null!;

        [Required]
        [Column("provider")]
        [MaxLength(191)]
        public string Provider { get; set; } = null!;

        [Required]
        [Column("provider_account_id")]
        [MaxLength(191)]
        public string ProviderAccountId { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;
    }
}
