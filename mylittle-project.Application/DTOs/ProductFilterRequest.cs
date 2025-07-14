using System.Collections.Generic;

namespace mylittle_project.Application.DTOs
{
    public class ProductFilterRequest
    {
        public string? CategoryName { get; set; }
        public string? BrandName { get; set; }

        public List<string>? Tags { get; set; }

        /// <summary>
        /// Filters based on dynamic product fields.
        /// Key = Field Name (NOT Field ID).
        /// Value = List of filter values (supports partial match).
        /// Example: { "Color": ["Blue", "Black"], "Weight": ["240g"] }
        /// </summary>
        public Dictionary<string, List<string>> FieldValues { get; set; } = new();

        public double? MinRating { get; set; }

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public bool Summary { get; set; } = false;
    }
}
