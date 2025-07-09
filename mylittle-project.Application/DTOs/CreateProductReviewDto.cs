using System;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Application.DTOs
{
    public class CreateProductReviewDto
    {
        [Required(ErrorMessage = "ProductId is required.")]
        public Guid ProductId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(150, ErrorMessage = "Title cannot exceed 150 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Review text is required.")]
        [StringLength(1000, ErrorMessage = "Review text cannot exceed 1000 characters.")]
        public string ReviewText { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }
    }
}
