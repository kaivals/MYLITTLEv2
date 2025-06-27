using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mylittle_project.Application.DTOs
{
    public class CreateUpdateCategoryDto
    {
        public string Name { get; set; } = string.Empty; // e.g. "Clothing"
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Parent { get; set; }
        public string Status { get; set; } = string.Empty; // "published" or "draft"
        public List<string>? AssignedFilters { get; set; }
    }
}
