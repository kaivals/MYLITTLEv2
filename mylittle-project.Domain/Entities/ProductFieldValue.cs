 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mylittle_project.Domain.Entities
{
    public class ProductFieldValue : BaseEntity
    {
       
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public Guid FieldId { get; set; }
        public ProductField? Field { get; set; }

        public string Value { get; set; } = string.Empty;

       
    }
}
