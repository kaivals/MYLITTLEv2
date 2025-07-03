using mylittle_project.Domain.Entities;

namespace mylittle_project.Domain.Entities
{
    public class Filter : AuditableEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty; // Example: "Color", "Price Range"
        public List<string> Values { get; set; } = new(); // Example: ["Red", "Blue"] or ["0-1000", "1000-2000"]
        public DateTime Created { get; set; }
        public Guid TenantId { get; set; }


        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}
