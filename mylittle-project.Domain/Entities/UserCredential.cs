using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mylittle_project.Domain.Entities
{
    public class UserCredential
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty; // FK to IdentityUser

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        public DateTime LoginTimestamp { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;


        [Required]
        public DateTime ExpiresAt { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
