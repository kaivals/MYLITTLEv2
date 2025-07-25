﻿using System;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Application.DTOs
{
    public class BrandProductDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Brand name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required.")]
        [RegularExpression("^(Active|Inactive)$", ErrorMessage = "Status must be either 'Active' or 'Inactive'.")]
        public string Status { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Order must be a positive number.")]
        public int Order { get; set; }

        public DateTime Created { get; set; }
        public Guid TenantId { get; set; }


        public string? LogoUrl { get; set; } // ✅ Add this line
    }
}
