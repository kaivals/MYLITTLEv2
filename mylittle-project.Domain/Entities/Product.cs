using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mylittle_project.Domain.Entities
{
    public class Product
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(150, ErrorMessage = "Product name can't exceed 150 characters.")]
        public string ProductName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Category can't exceed 100 characters.")]
        public string Category { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Brand name can't exceed 100 characters.")]
        public string Brand { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative.")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock must be non-negative.")]
        public int Stock { get; set; }

        [StringLength(50, ErrorMessage = "Status can't exceed 50 characters.")]
        public string Status { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Description can't exceed 2000 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "TenantId is required.")]
        [ForeignKey("Tenant")]
        public Guid TenantId { get; set; }

        public Tenant Tenant { get; set; } = null!;
    }
}
