using System;
using System.Collections.Generic;

namespace mylittle_project.Domain.Entities
{
    public class Filter : AuditableEntity
    {
        public Guid Id { get; set; }

        public Guid TenantId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = "dropdown";
        // dropdown, multiselect, toggle, slider, range-slider

        public bool IsDefault { get; set; } = false;

        public string Description { get; set; } = string.Empty;

        public List<string> Values { get; set; } = new();

        public string Status { get; set; } = "active";
        // active or inactive

        public DateTime Created { get; set; }

        public int UsageCount { get; set; } = 0;

        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        public ICollection<Category> Categories { get; set; } = new List<Category>();
        
    }
}
