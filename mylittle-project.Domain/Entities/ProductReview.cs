using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mylittle_project.Domain.Entities
{
    public class ProductReview
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; }= string.Empty;

        [Required]
        public string ReviewText { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; }

        public bool IsApproved { get; set; }
        public bool IsVerified { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        // Optional: Navigation property
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}
