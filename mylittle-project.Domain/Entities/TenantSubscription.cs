using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mylittle_project.Domain.Entities
{
    public class TenantSubscription
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public Guid GlobalPlanId { get; set; }

        public string PlanName { get; set; } = string.Empty;
        public decimal PlanCost { get; set; }
        public int NumberOfAds { get; set; }
        public int MaxEssentialMembers { get; set; }
        public int MaxPremiumMembers { get; set; }
        public int MaxEliteMembers { get; set; }
        public bool IsTrial { get; set; }
        public bool IsActive { get; set; }
        public GlobalSubscription GlobalPlan { get; set; } = null!;
      
    }
}
