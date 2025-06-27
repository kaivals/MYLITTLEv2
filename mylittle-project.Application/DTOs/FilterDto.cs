using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject.Application.DTOs
{
    public class FilterDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<string> Values { get; set; }
        public DateTime Created { get; set; }
    }
}