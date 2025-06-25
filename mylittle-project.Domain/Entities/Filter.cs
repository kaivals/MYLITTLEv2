using mylittle_project.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;

public class Filter
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Filter name is required.")]
    [StringLength(100, ErrorMessage = "Filter name can't exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description can't exceed 500 characters.")]
    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    [Required(ErrorMessage = "Filter type is required.")]
    [RegularExpression("^(toggle|range|multiselect)$", ErrorMessage = "Type must be one of: toggle, range, multiselect.")]
    public string Type { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required.")]
    public string Category { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "RangeStart must be non-negative.")]
    public double? RangeStart { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "RangeEnd must be non-negative.")]
    public double? RangeEnd { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Step must be non-negative.")]
    public double? Step { get; set; }

    [StringLength(500, ErrorMessage = "Options can't exceed 500 characters.")]
    public string? Options { get; set; }

    [Required(ErrorMessage = "StoreId is required.")]
    public Guid StoreId { get; set; }

    public Store? Store { get; set; }
}
