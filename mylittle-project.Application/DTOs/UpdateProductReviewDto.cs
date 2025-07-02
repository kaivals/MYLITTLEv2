using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mylittle_project.Application.DTOs
{
    public class UpdateProductReviewDto
    {
        public string Title { get; set; }
        public string ReviewText { get; set; }
        public int Rating { get; set; }
        public bool IsApproved { get; set; }
        public bool IsVerified { get; set; }
    }
}

