using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Application.DTOs
{
    public class ProductSectionDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Section name is required.")]
        [MaxLength(100, ErrorMessage = "Section name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

       
    }
}
