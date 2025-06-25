using System;

namespace mylittle_project.Domain.Entities
{
    /// <summary>Represents a tenant’s ON/OFF switch for an individual feature.</summary>
    public class TenantFeature
    {
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = default!;

        public Guid FeatureId { get; set; }
        public Feature Feature { get; set; } = default!;

        public bool IsEnabled { get; set; }

        // helps with queries/cascade; not a separate FK – kept in-sync by code
        public Guid ModuleId { get; set; }
    }
}
