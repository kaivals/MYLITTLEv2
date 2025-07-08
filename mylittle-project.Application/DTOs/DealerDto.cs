using System;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Application.DTOs
{
    public class DealerDto
    {
        [Required(ErrorMessage = "TenantId is required.")]
        public Guid TenantId { get; set; }

        [Required(ErrorMessage = "DealerName is required.")]
        [StringLength(100, ErrorMessage = "DealerName cannot exceed 100 characters.")]
        public string DealerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "BusinessName is required.")]
        [StringLength(150, ErrorMessage = "BusinessName cannot exceed 150 characters.")]
        public string BusinessName { get; set; } = string.Empty;

        [Required(ErrorMessage = "BusinessNumber is required.")]
        [StringLength(20, ErrorMessage = "BusinessNumber cannot exceed 20 characters.")]
        public string BusinessNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "BusinessEmail is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [MaxLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        public string BusinessEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "BusinessAddress is required.")]
        [StringLength(250, ErrorMessage = "BusinessAddress cannot exceed 250 characters.")]
        public string BusinessAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [MaxLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        public string ContactEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        [MaxLength(15, ErrorMessage = "Phone number cannot exceed 15 digits.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Url(ErrorMessage = "Invalid website URL.")]
        [StringLength(200, ErrorMessage = "Website URL is too long.")]
        public string Website { get; set; } = string.Empty;

        [Required(ErrorMessage = "TaxIdOrGstNumber is required.")]
        [StringLength(50, ErrorMessage = "TaxIdOrGstNumber cannot exceed 50 characters.")]
        public string TaxIdOrGstNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required.")]
        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters.")]
        public string Country { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required.")]
        [StringLength(100, ErrorMessage = "State cannot exceed 100 characters.")]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Timezone is required.")]
        [StringLength(100, ErrorMessage = "Timezone cannot exceed 100 characters.")]
        public string Timezone { get; set; } = string.Empty;
        public string? PortalName { get; set; }  // Optional: included for Tenant Owner only
    }
}
