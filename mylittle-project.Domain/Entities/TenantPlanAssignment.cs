using mylittle_project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mylittle_project.Domain.Entities
{
    public class TenantPlanAssignment
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public Tenant? Tenant { get; set; }


        public Guid CategoryId { get; set; }
        public Category? Category { get; set; }


        public Guid DealerId { get; set; }
        public BusinessInfo? Dealer { get; set; }


        public string PlanType { get; set; } = string.Empty; // Premium / Elite / Essential

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string Status { get; set; } = "Active"; // Active / Expired / Upcoming

        public int SlotsUsed { get; set; }
        public int MaxSlots { get; set; }
    }

}
