using mylittle_project.Domain.Entities;
using System.ComponentModel.DataAnnotations;

public class GlobalSubscription : AuditableEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Plan name is required.")]
    [StringLength(100, ErrorMessage = "Plan name cannot exceed 100 characters.")]
    public string PlanName { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string Description { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "Plan cost must be a non-negative value.")]
    public decimal PlanCost { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Number of ads must be non-negative.")]
    public int NumberOfAds { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Max members must be non-negative.")]
    public int MaxMembers { get; set; }  // Single property for all plans

    public bool IsTrial { get; set; }
    public bool IsActive { get; set; }
}
