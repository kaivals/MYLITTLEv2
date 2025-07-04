using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Application.DTOs
{
    public class FilterDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Filter name is required.")]
        [StringLength(100, ErrorMessage = "Filter name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Filter type is required.")]
        [RegularExpression("dropdown|multiselect|toggle|slider|range-slider",
            ErrorMessage = "Type must be one of: dropdown, multiselect, toggle, slider, range-slider.")]
        public string Type { get; set; } = "dropdown";

        public bool IsDefault { get; set; }

        [StringLength(300, ErrorMessage = "Description cannot exceed 300 characters.")]
        public string Description { get; set; } = string.Empty;

        [MinLength(1, ErrorMessage = "At least one value is required.")]
        public List<string> Values { get; set; } = new();

        [Required(ErrorMessage = "Status is required.")]
        [RegularExpression("active|inactive", ErrorMessage = "Status must be either 'active' or 'inactive'.")]
        public string Status { get; set; } = "active";

        [Range(0, int.MaxValue, ErrorMessage = "Usage count must be zero or more.")]
        public int UsageCount { get; set; }

        public DateTime Created { get; set; }

        public DateTime LastModified { get; set; }
    }
}
