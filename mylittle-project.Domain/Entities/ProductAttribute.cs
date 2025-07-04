using System;

namespace mylittle_project.Domain.Entities
{
    public class ProductAttribute
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string Name { get; set; } = null!;
        public string FieldType { get; set; } = null!;
        public bool IsRequired { get; set; }
        public bool IsVisible { get; set; }
        public bool IsFilterable { get; set; }
        public bool IsVariantOption { get; set; }
        public string? Options { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Source { get; set; }
        public string? SectionType { get; set; }
    }
}
