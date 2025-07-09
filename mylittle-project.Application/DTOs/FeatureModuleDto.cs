using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Application.DTOs
{
    public class FeatureModuleDto
    {
        [Required]
        public Guid ModuleId { get; set; }

        [Required(ErrorMessage = "Module name is required.")]
        [StringLength(150, ErrorMessage = "Module name cannot exceed 150 characters.")]
        public string Name { get; set; } = default!;

        public bool IsEnabled { get; set; }

        public List<FeatureToggleDto> Features { get; set; } = new();
    }
}
