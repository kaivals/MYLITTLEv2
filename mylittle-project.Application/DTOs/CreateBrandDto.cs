using System;

namespace mylittle_project.Application.DTOs
{
    public class CreateBrandDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; } // "Active" or "Inactive"
        public int Order { get; set; }
    }
}
