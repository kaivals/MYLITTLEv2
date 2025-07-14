using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Application.DTOs
{

    public class ProductDto
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category ID is required.")]
        public List<Guid> CategoryIds { get; set; } = new();

        public Guid? BrandId { get; set; }

        
        /// <summary>
        /// Dynamic product fields and their values.
        /// Key = Field Name (NOT Field ID).
        /// Value = Field value.
        /// Example: { "Color": "Blue", "Weight": "240g" }
        /// </summary>
        public Dictionary<string, string>? FieldValues { get; set; }

        public decimal? Price { get; set; }


        // Optional tags
        public List<Guid>? TagIds { get; set; }
    }
}
