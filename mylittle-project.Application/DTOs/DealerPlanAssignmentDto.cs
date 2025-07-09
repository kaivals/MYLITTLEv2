using System;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Application.DTOs
{
    public class DealerPlanAssignmentDto
    {
        [Required(ErrorMessage = "CategoryId is required.")]
        public Guid CategoryId { get; set; }

        [Required(ErrorMessage = "DealerId is required.")]
        public Guid DealerId { get; set; }

        [Required(ErrorMessage = "PlanType is required.")]
        [StringLength(100, ErrorMessage = "PlanType cannot exceed 100 characters.")]
        public string PlanType { get; set; } = string.Empty;

        [Required(ErrorMessage = "StartDate is required.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "EndDate is required.")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [RegularExpression("^(Active|Inactive|Upcoming|Expired)$", ErrorMessage = "Status must be Active, Inactive, Upcoming, or Expired.")]
        public string Status { get; set; } = "Active";

        [Range(0, int.MaxValue, ErrorMessage = "SlotsUsed must be a non-negative number.")]
        public int SlotsUsed { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "MaxSlots must be a non-negative number.")]
        public int MaxSlots { get; set; }
    }

    public class SchedulerAssignmentDto
    {
        public string Category { get; set; } = string.Empty;
        public string Dealer { get; set; } = string.Empty;
        public string PlanType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
