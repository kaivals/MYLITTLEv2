using mylittle_project.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mylittle_project.Domain.Entities

{
    public class Product
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(150)]
        public string ProductName { get; set; } = string.Empty;

        [StringLength(100)]
        public string Brand { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int Stock { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public Guid TenantId { get; set; }

        public Tenant Tenant { get; set; } = null!;

        [Required]
        public Guid CategoryId { get; set; }

        public Category Category { get; set; } = null!;
    }
}
