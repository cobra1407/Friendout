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

        [Column("refresh_token", TypeName = "text")]
        public string? RefreshToken { get; set; }

        [Column("access_token", TypeName = "text")]
        public string? AccessToken { get; set; }

        [Column("expires_at")]
        public long? ExpiresAt { get; set; }

        [Column("token_type")]
        [MaxLength(191)]
        public string? TokenType { get; set; }

        [Column("scope")]
        [MaxLength(191)]
        public string? Scope { get; set; }

        [Column("id_token", TypeName = "text")]
        public string? IdToken { get; set; }

        [Column("session_state")]
        [MaxLength(191)]
        public string? SessionState { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;
    }
}
