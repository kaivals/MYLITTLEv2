using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mylittle_project.Domain.Entities
{
    public class DealerSubscription
    {
        public Guid Id { get; set; }

        public Guid DealerId { get; set; }
        public Dealer? Dealer { get; set; }

        public Guid TenantId { get; set; }
        public Tenant? Tenant { get; set; }

        public Guid CategoryId { get; set; }
        public Category? Category { get; set; }

        public string PlanType { get; set; } = string.Empty; // Essential / Premium / Elite

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool IsQueued { get; set; } = false;
        public string Status { get; set; } = "Upcoming"; // Active / Upcoming / Expired / Queued

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
