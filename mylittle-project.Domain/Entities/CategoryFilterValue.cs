using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mylittle_project.Domain.Entities
{
    public class CategoryFilterValue
    {
        public Guid Id { get; set; }

        public Guid CategoryId { get; set; }
        public Category? Category { get; set; } // Navigation property to Category

        public string FilterName { get; set; } = string.Empty;  // e.g. "brand"

        public string Value { get; set; } = string.Empty;  // e.g. "Nike", "Adidas"
    }

}
