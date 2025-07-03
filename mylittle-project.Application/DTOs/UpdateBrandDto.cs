using System;

namespace mylittle_project.Application.DTOs
{
    public class UpdateBrandDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int Order { get; set; }
    }
}
