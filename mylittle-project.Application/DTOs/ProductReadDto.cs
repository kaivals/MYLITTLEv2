using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mylittle_project.Application.DTOs
{
    public class ProductReadDto
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public Dictionary<string, object> Attributes { get; set; } = new();
        public Guid CategoryId { get; set; }
        public string CreatedAt { get; set; }
        public string Data { get; set; }
    }

}
