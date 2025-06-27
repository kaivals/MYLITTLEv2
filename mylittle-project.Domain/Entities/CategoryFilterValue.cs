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
        public Category Category { get; set; }

        public string FilterName { get; set; }  // e.g. "brand"

        public string Value { get; set; }  // e.g. "Nike", "Adidas"
    }

}
