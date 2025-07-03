using System;

namespace mylittle_project.Domain.Entities
{
    public class Brand
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; } // "Active" or "Inactive"
        public int Order { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}

