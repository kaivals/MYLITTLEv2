using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mylittle_project.Application.DTOs
{
    public class ProductFieldDto
    {
        public string Name { get; set; } = string.Empty;

        // Examples: text, number, dropdown, checkbox, textarea, date
        public string FieldType { get; set; } = string.Empty;

        public Guid SectionId { get; set; }

        public bool VisibleToDealer { get; set; } = true;
        public bool IsRequired { get; set; } = false;
        public bool AutoSyncEnabled { get; set; } = false;

        // Optional: For dropdown types, allow predefined options
        public List<string>? Options { get; set; } // Used when FieldType == "dropdown"
        public object Id { get; set; }
        public bool IsVisibleToDealer { get; set; }
    }


}
