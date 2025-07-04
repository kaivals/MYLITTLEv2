using System;
using System.Collections.Generic;

namespace mylittle_project.Domain.Entities
{
    public class    ProductField
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string FieldType { get; set; } = string.Empty; // e.g., Text, Dropdown, etc.

        public Guid SectionId { get; set; }
        public ProductSection Section { get; set; } = null!;

        public bool IsRequired { get; set; }         // ✅ Toggle: Required
        public bool IsFilterable { get; set; }       // ✅ Toggle: Filtering
        public bool IsVariantOption { get; set; }    // ✅ Toggle: Variants
        public bool IsVisible { get; set; }          // ✅ Toggle: Visible

        public bool AutoSyncEnabled { get; set; }    // ✅ Already present

        public List<string>? Options { get; set; }   // Optional: for dropdown/multiselect

        public bool IsDeleted { get; set; } = false;
        public bool IsVisibleToDealer { get; set; }

        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
