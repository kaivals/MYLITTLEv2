using System;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Application.DTOs
{
    public class FeatureToggleDto
    {
        [Required]
        public Guid FeatureId { get; set; }

        [Required(ErrorMessage = "Feature name is required.")]
        [StringLength(150, ErrorMessage = "Feature name cannot exceed 150 characters.")]
        public string Name { get; set; } = default!;

        public bool IsEnabled { get; set; }
    }
}
