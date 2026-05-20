using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Friendout.Domain.Models
{
    [Table("sessions")]
    public class Session
    {
        [Key]
        [Column("id")]
        [MaxLength(191)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Column("session_token")]
        [MaxLength(255)]
        public string SessionToken { get; set; } = null!;


        [Required]
        [Column("user_id")]
        [MaxLength(191)]
        public string UserId { get; set; } = null!;

        [Required]
        [Column("expires")]
        public DateTime Expires { get; set; }

        // Relations
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}
