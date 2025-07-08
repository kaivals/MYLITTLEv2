namespace mylittle_project.Application.DTOs
{
    public class TenantSubscriptionDto
    {
        public string PlanName { get; set; } = string.Empty;
        public decimal PlanCost { get; set; }
        public int NumberOfAds { get; set; }
        public int MaxMembers { get; set; }  // Unified field
        public bool IsTrial { get; set; }
        public bool IsActive { get; set; }
    }
}
