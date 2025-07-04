using System;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Domain.Entities
{
    public class ProductTag
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        public bool Published { get; set; } = true;

        public int TaggedProducts { get; set; } = 0;

        public DateTime Created { get; set; } = DateTime.UtcNow;

        // ✅ Correct relationship
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}
