using System;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Domain.Entities
{
    public class Subscription
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "TenantId is required.")]
        public Guid TenantId { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; }

        [Required(ErrorMessage = "Plan name is required.")]
        [StringLength(100, ErrorMessage = "Plan name cannot exceed 100 characters.")]
        public string PlanName { get; set; } = string.Empty;

        public bool IsTrial { get; set; }
    }
}
