using mylittle_project.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class ProductTag : AuditableEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Tag name is required.")]
    [MaxLength(100, ErrorMessage = "Tag name cannot exceed 100 characters.")]
    public string Name { get; set; } = null!;

    public bool Published { get; set; } = true;

    [Range(0, int.MaxValue, ErrorMessage = "Tagged product count cannot be negative.")]
    public int TaggedProducts { get; set; } = 0;
    public DateTime Created { get; set; } = DateTime.UtcNow;

    [JsonIgnore] // ✅ Prevents serialization loop if entity accidentally returned
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
