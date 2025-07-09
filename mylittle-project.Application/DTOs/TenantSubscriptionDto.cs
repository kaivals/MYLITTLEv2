using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Application.DTOs
{
    public class TenantSubscriptionDto
    {
        [Required(ErrorMessage = "Plan name is required.")]
        public string PlanName { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Plan cost cannot be negative.")]
        public decimal PlanCost { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Number of ads cannot be negative.")]
        public int NumberOfAds { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Max members must be at least 1.")]
        public int MaxMembers { get; set; }

        public bool IsTrial { get; set; }
        public bool IsActive { get; set; }
    }
}
