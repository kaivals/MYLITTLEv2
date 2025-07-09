using System;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Application.DTOs
{
    public class CreateUpdateCategoryDto
    {
        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(150, ErrorMessage = "Category name cannot exceed 150 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "Slug cannot exceed 150 characters.")]
        public string Slug { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        public Guid? ParentId { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [RegularExpression("^(published|draft)$", ErrorMessage = "Status must be 'published' or 'draft'.")]
        public string Status { get; set; } = string.Empty;
    }
}
