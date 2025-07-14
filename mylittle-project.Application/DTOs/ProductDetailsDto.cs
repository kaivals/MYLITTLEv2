using System;
using System.Collections.Generic;

namespace mylittle_project.Application.DTOs
{
    public class ProductDetailsDto
    {
        
        public string Name { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public List<string>? Categories { get; set; }
        /// <summary>
        /// Dynamic product fields with their stored values.
        /// Key = Field Name.
        /// Value = Field value.
        /// Example: { "Color": "Blue", "Weight": "240g" }
        /// </summary>
        public Dictionary<string, string>? FieldValues { get; set; }


        public List<ProductTagDto>? Tags { get; set; }
        public List<ProductReviewDto>? Reviews { get; set; }

        public BrandProductDto? Brand { get; set; }
    }
}
