using System;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Application.DTOs
{
    public class ProductDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Category ID is required.")]
        public Guid CategoryId { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        public string ProductName { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be a positive value.")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
        public int Stock { get; set; }

        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tenant ID is required.")]
        public Guid TenantId { get; set; }
    }
}
