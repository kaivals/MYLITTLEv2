using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject.Application.DTOs
{
    public class CreateUpdateFilterDto
    {
        public string Name { get; set; }
        public List<string> Values { get; set; }
    }
}

