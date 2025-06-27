using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Domain/Entities/AssignedFilter.cs
namespace mylittle_project.Domain.Entities
{
    public class AssignedFilter
    {
        public string Name { get; set; }
        public List<string> Values { get; set; } = new();
    }
}

