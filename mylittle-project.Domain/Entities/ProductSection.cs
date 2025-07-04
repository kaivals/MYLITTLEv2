using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mylittle_project.Domain.Entities
{
    public class ProductSection
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }

        public string Title { get; set; } = string.Empty; // e.g., "Product Info", "Shipping"

        public bool IsDeleted { get; set; } = false;

        public ICollection<ProductField> Fields { get; set; } = new List<ProductField>();
        public string Name { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }



}
