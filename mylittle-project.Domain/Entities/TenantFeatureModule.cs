using System;
using System.Collections.Generic;

namespace mylittle_project.Domain.Entities
{
    /// <summary>Represents a tenant’s ON/OFF switch for one entire module.</summary>
    public class TenantFeatureModule
    {
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = default!;

        public Guid ModuleId { get; set; }
        public FeatureModule Module { get; set; } = default!;

        public bool IsEnabled { get; set; }

        // convenience navigation to quickly reach children
        public ICollection<TenantFeature> TenantFeatures { get; set; } = new List<TenantFeature>();
    }
}
