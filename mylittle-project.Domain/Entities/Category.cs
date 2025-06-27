using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Domain.Entities
{
    public class Category
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty; // e.g. "Clothing"
        public string Slug { get; set; } = string.Empty;          // e.g. #clothing
        public string? Description { get; set; }
        public string? Parent { get; set; }       // e.g. "Electronics"

        public int ProductCount { get; set; }     // auto-updated count
        public string Status { get; set; } = string.Empty;        // published / draft

        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public List<string> AssignedFilters { get; set; } = new(); // Category, Brand, Size, etc.
        public Guid TenantId { get; set; }
    }
}
