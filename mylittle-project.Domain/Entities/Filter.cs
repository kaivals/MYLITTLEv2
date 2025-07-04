using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Domain.Entities
{
    public class Filter : AuditableEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "TenantId is required.")]
        public Guid TenantId { get; set; }

        [Required(ErrorMessage = "Filter name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type is required.")]
        [RegularExpression("dropdown|multiselect|toggle|slider|range-slider",
            ErrorMessage = "Type must be one of: dropdown, multiselect, toggle, slider, range-slider.")]
        public string Type { get; set; } = "dropdown";

        public bool IsDefault { get; set; } = false;

        [StringLength(300, ErrorMessage = "Description cannot exceed 300 characters.")]
        public string Description { get; set; } = string.Empty;

        [MinLength(1, ErrorMessage = "At least one value must be provided.")]
        public List<string> Values { get; set; } = new();

        [Required(ErrorMessage = "Status is required.")]
        [RegularExpression("active|inactive", ErrorMessage = "Status must be either 'active' or 'inactive'.")]
        public string Status { get; set; } = "active";

        public DateTime Created { get; set; } = DateTime.UtcNow;

        [Range(0, int.MaxValue, ErrorMessage = "UsageCount cannot be negative.")]
        public int UsageCount { get; set; } = 0;

        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}
