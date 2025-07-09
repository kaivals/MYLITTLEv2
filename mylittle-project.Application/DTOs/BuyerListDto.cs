using System;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Application.DTOs
{
    public class BuyerListDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Buyer name is required.")]
        [StringLength(150, ErrorMessage = "Buyer name cannot exceed 150 characters.")]
        public string BuyerName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number.")]
        public string PhoneNumber { get; set; } = string.Empty;

        public string PortalName { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Total orders must be a non-negative number.")]
        public int TotalOrders { get; set; }

        [Required]
        public Guid TenantId { get; set; }

        [Required]
        public Guid? DealerId { get; set; }

        public bool IsActive { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } = string.Empty;

        public string TenantName { get; set; } = string.Empty;
    }
}
