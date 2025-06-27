using System;

namespace MyProject.Application.DTOs
{
    public class CategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Slug { get; set; }
        public string? Description { get; set; }

        public Guid? ParentId { get; set; }
        public string? ParentName { get; set; }

        public int ProductCount { get; set; }
        public int FilterCount { get; set; }

        public string Status { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}
