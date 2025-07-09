using System;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Application.DTOs
{
    public class BuyerSummaryDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters.")]
        public string Name { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number.")]
        public string Phone { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Country name cannot exceed 100 characters.")]
        public string Country { get; set; } = string.Empty;

        public DateTime RegistrationDate { get; set; }
        public DateTime LastLogin { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [RegularExpression("^(Active|Inactive)$", ErrorMessage = "Status must be 'Active' or 'Inactive'.")]
        public string Status { get; set; } = "Active";

        public bool IsActive { get; set; }

        [Required]
        public Guid? DealerId { get; set; }

        [Required]
        public Guid TenantId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Total orders must be a non-negative number.")]
        public int TotalOrders { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Total activities must be a non-negative number.")]
        public int TotalActivities { get; set; }
    }
}
