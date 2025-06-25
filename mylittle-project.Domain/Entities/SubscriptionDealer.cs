using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Domain.Entities
{
    public class SubscriptionDealer
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "TenantId is required.")]
        public Guid TenantId { get; set; }

        [Required(ErrorMessage = "Portal name is required.")]
        [StringLength(100, ErrorMessage = "Portal name can't exceed 100 characters.")]
        public string PortalName { get; set; } = string.Empty;

        public List<AssignedCategory> Categories { get; set; } = new();

        [Required(ErrorMessage = "Subscription plan is required.")]
        [StringLength(100, ErrorMessage = "Subscription plan can't exceed 100 characters.")]
        public string SubscriptionPlan { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0.")]
        public decimal Price { get; set; }

        [Range(1, 60, ErrorMessage = "Duration must be between 1 and 60 months.")]
        public int DurationInMonths { get; set; }

        public bool AutoRenew { get; set; }

        public bool QueueIfUnavailable { get; set; }

        public DateTime PlanStartDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "BusinessId is required.")]
        public Guid BusinessId { get; set; }

        public BusinessInfo? BusinessInfo { get; set; }
    }

    public class AssignedCategory
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100, ErrorMessage = "Category name can't exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        public bool IsAvailable { get; set; }
    }
}
