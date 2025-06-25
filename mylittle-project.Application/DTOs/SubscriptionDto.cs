using System;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Application.DTOs
{
    public class SubscriptionDto
    {
        [Required, MaxLength(100)]
        public string PlanName { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsTrial { get; set; }

        public bool IsActive { get; set; }
    }
}
