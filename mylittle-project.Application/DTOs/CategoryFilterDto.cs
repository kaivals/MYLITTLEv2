using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mylittle_project.Application.DTOs
{
    // DTOs/CategoryFilterDto.cs
    public class CategoryFilterDto
    {
        public string? Name { get; set; }
        public Guid? ParentId { get; set; }
        public bool? HasProducts { get; set; }
        public bool? HasFilters { get; set; }
        public string? Status { get; set; }  
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public string? SortBy { get; set; } = "Created";
        public string? SortDirection { get; set; } = "desc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

}
