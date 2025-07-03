using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mylittle_project.Application.DTOs
{
    public class ProductReviewDto
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; } // Optional for display
        public string Title { get; set; }
        public string ReviewText { get; set; }
        public int Rating { get; set; }
        public bool IsApproved { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}

