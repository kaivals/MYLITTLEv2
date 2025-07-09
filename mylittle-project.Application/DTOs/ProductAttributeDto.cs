using System;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Application.DTOs
{
    public class ProductAttributeDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Attribute name is required.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Field type is required.")]
        public string FieldType { get; set; } = string.Empty;

        public bool IsRequired { get; set; }
        public bool IsVisible { get; set; }
        public bool IsFilterable { get; set; }
        public bool IsVariantOption { get; set; }
        public bool IsAutoSynced { get; set; }

        public string? Options { get; set; }
    }
}
