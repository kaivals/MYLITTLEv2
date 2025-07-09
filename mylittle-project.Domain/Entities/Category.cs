using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Domain.Entities
{
    public class Category : AuditableEntity
    {
        public Guid Id { get; set; }

        [Required]
        public Guid TenantId { get; set; }

        [Required]
        [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "Slug cannot exceed 150 characters.")]
        public string? Slug { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        public Guid? ParentId { get; set; }
        public Category? Parent { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Product count must be a non-negative number.")]
        public int ProductCount { get; set; }

        [Required]
        [RegularExpression("^(Active|Inactive)$", ErrorMessage = "Status must be either 'Active' or 'Inactive'.")]
        public string Status { get; set; } = string.Empty;

        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<Filter> Filters { get; set; } = new List<Filter>();
    }
}
