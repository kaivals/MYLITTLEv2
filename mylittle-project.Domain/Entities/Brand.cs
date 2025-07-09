using System;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Domain.Entities
{
    public class Brand
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(Active|Inactive)$", ErrorMessage = "Status must be either 'Active' or 'Inactive'.")]
        public string Status { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Order must be a positive number.")]
        public int Order { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; }
    }
}
