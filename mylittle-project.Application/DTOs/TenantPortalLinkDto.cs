using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Application.DTOs
{
    public class TenantPortalLinkDto
    {
        public Guid Id { get; set; }

        [Required]
        public Guid SourceTenantId { get; set; }

        [Required]
        public Guid TargetTenantId { get; set; }

        [Required]
        [StringLength(50)]
        public string LinkType { get; set; } = string.Empty;

        public DateTime? LinkedSince { get; set; } = DateTime.UtcNow;
    }
}
