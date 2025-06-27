namespace MyProject.Domain.Entities
{
    public class Filter
    {
        public Guid Id { get; set; }
        public string Name { get; set; }           // Example: "Color", "Price Range"
        public List<string> Values { get; set; } = new(); // Example: ["Red", "Blue"] or ["0-1000", "1000-2000"]
        public DateTime Created { get; set; }
        public Guid TenantId { get; set; }
    }
}
