using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Application.DTOs
{
    public class SubscriptionDealerDto
    {
        [Required]
        public Guid TenantId { get; set; }

        [Required, MaxLength(100)]
        public string PortalName { get; set; } = string.Empty;

        [MinLength(1)]
        public List<AssignedCategoryDto> Categories { get; set; } = new();

        [Required]
        public string SubscriptionPlan { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(1, 60)]
        public int DurationInMonths { get; set; }

        public bool AutoRenew { get; set; }
        public bool QueueIfUnavailable { get; set; }

        [Required]
        public DateTime PlanStartDate { get; set; }

        [Required]
        public Guid BusinessId { get; set; }
    }

    public class AssignedCategoryDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool IsAvailable { get; set; }
    }
}
