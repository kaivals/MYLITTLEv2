using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mylittle_project.Application.DTOs
{
    public class CreateFilterDto
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsDefault { get; set; }
        public string Description { get; set; }
        public List<string> Values { get; set; }
        public string Status { get; set; }
    }

}
