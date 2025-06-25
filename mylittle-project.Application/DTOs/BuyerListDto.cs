using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mylittle_project.Application.DTOs
{
    public class BuyerListDto
    {
        public Guid Id { get; set; }

        public string BuyerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        public int TotalOrders { get; set; }
        public Guid TenantId { get; set; }
        public Guid BusinessId { get; set; }

        public bool IsActive { get; set; }
        public string Status { get; set; }= string.Empty;
    }
}
