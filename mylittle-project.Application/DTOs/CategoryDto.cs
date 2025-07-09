using System;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Application.DTOs
{
    public class CategoryDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "Slug cannot exceed 150 characters.")]
        public string? Slug { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        public Guid? ParentId { get; set; }
        public string? ParentName { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Product count must be a non-negative number.")]
        public int ProductCount { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Filter count must be a non-negative number.")]
        public int FilterCount { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [RegularExpression("^(Active|Inactive)$", ErrorMessage = "Status must be 'Active' or 'Inactive'.")]
        public string Status { get; set; } = string.Empty;

        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}
