using mylittle_project.Domain.Entities;
using System.ComponentModel.DataAnnotations;

public class TenantSubscription : AuditableEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "TenantId is required.")]
    public Guid TenantId { get; set; }

    [Required(ErrorMessage = "GlobalPlanId is required.")]
    public Guid GlobalPlanId { get; set; }

    [Required(ErrorMessage = "Plan name is required.")]
    [StringLength(100, ErrorMessage = "Plan name cannot exceed 100 characters.")]
    public string PlanName { get; set; } = string.Empty;

    [Range(0.0, double.MaxValue, ErrorMessage = "Plan cost must be non-negative.")]
    public decimal PlanCost { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Number of ads must be zero or more.")]
    public int NumberOfAds { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Max members must be zero or more.")]
    public int MaxMembers { get; set; }  // Unified field

    public bool IsTrial { get; set; }
    public bool IsActive { get; set; }

    [Required]
    public GlobalSubscription GlobalPlan { get; set; } = null!;
}
