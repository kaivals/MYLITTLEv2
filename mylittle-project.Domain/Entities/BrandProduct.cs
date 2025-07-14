using System;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Domain.Entities
{
    public class BrandProduct : AuditableEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(Active|Inactive)$")]
        public string Status { get; set; } = string.Empty;

        [Required]
        public Guid TenantId { get; set; }

        [Range(0, int.MaxValue)]
        public int Order { get; set; }

        [StringLength(300)]
        public string? LogoUrl { get; set; }  // ✅ Logo file path or URL
    }
}
