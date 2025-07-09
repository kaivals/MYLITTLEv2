using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Application.DTOs
{
    public class ProductFieldDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Section ID is required.")]
        public Guid SectionId { get; set; }

        [Required(ErrorMessage = "Field name is required.")]
        [MaxLength(100, ErrorMessage = "Field name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Field type is required.")]
        [MaxLength(50, ErrorMessage = "Field type cannot exceed 50 characters.")]
        public string FieldType { get; set; } = string.Empty;

        public bool IsRequired { get; set; } = false;
        public bool AutoSyncEnabled { get; set; } = false;
        public bool IsVisibleToDealer { get; set; } = true;
        public bool VisibleToDealer { get; set; } = true;
        public bool IsFilterable { get; set; } = false;
        public bool IsVariantOption { get; set; } = false;
        public bool IsVisible { get; set; } = true;

        public List<string>? Options { get; set; }
    }
}
