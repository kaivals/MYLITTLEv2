using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Application.DTOs
{
    public class UpdateBrandDto
    {
        [Required(ErrorMessage = "Brand name is required.")]
        [MaxLength(100, ErrorMessage = "Brand name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required.")]
        [RegularExpression("^(Active|Inactive)$", ErrorMessage = "Status must be 'Active' or 'Inactive'.")]
        public string Status { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Order must be a non-negative integer.")]
        public int Order { get; set; }
    }
}
