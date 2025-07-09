using System;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Application.DTOs
{
    public class ProductReviewDto
    {
        public Guid Id { get; set; }

        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Review title is required.")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Review text is required.")]
        public string ReviewText { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        public bool IsApproved { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
