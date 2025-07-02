using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mylittle_project.Domain.Entities
{
    public class ProductField
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string FieldType { get; set; } = string.Empty;

        public Guid SectionId { get; set; }
        public ProductSection Section { get; set; } = null!;

        
        public bool IsRequired { get; set; }
        public bool AutoSyncEnabled { get; set; }

        public List<string>? Options { get; set; } // Optional for dropdown

        public bool IsDeleted { get; set; } = false;
        public bool IsVisibleToDealer { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }




}
