using System;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Application.DTOs
{
    public class DealerSubscriptionApplicationDto
    {
        [Required(ErrorMessage = "DealerId is required.")]
        public Guid DealerId { get; set; }

        [Required(ErrorMessage = "TenantId is required.")]
        public Guid TenantId { get; set; }

        [Required(ErrorMessage = "CategoryId is required.")]
        public Guid CategoryId { get; set; }

        [Required(ErrorMessage = "PlanType is required.")]
        [StringLength(100, ErrorMessage = "PlanType cannot exceed 100 characters.")]
        public string PlanType { get; set; } = string.Empty;

        [Required(ErrorMessage = "StartDate is required.")]
        public DateTime StartDate { get; set; }

        public bool IsQueued { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [RegularExpression("^(Pending|Active|Upcoming|Rejected)$", ErrorMessage = "Invalid Status.")]
        public string Status { get; set; } = "Upcoming";
    }
}
