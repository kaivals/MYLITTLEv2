using System;
using System.Collections.Generic;

namespace mylittle_project.Domain.Entities
{
    public class Category
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }

        public string Name { get; set; }
        public string? Slug { get; set; }
        public string? Description { get; set; }

        public Guid? ParentId { get; set; }
        public Category? Parent { get; set; }

        public int ProductCount { get; set; }
        public string Status { get; set; }

        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        // Navigation properties
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<Filter> Filters { get; set; } = new List<Filter>();
    }
}
