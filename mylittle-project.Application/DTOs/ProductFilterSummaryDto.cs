using System;
using System.Collections.Generic;

namespace mylittle_project.Application.DTOs
{
    public class ProductFilterSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public string? BrandName { get; set; }

        public double? AverageRating { get; set; }

        public string? ThumbnailUrl { get; set; }

        public decimal? Price { get; set; }

        public List<string>? Tags { get; set; }
        /// <summary>
        /// Dynamic product fields and their summary values.
        /// Key = Field Name.
        /// Value = Field value.
        /// Example: { "Color": "Blue", "Weight": "240g" }
        /// </summary>
        public Dictionary<string, string>? FieldValues { get; set; }

    }
}
