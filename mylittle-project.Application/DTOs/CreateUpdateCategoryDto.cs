using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject.Application.DTOs
{
    public class CreateUpdateCategoryDto
    {
        public string Name { get; set; }
        public string Slug { get; set; }
        public string? Description { get; set; }
        public string? Parent { get; set; }
        public string Status { get; set; } // "published" or "draft"
        public List<string> AssignedFilters { get; set; }
    }
}
