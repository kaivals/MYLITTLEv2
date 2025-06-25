using System;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Domain.Entities
{
    /// <summary>Concrete feature that can be toggled on/off per tenant.</summary>
    public class Feature
    {
        public Guid Id { get; set; }

        [Required]
        public Guid ModuleId { get; set; }

        public FeatureModule Module { get; set; } = default!;

        [Required]                   // unique key for look-ups
        public string Key { get; set; } = default!;

        [Required]
        public string Name { get; set; } = default!;

        public string? Description { get; set; }

        /// <summary>Marks “premium” items in UI – *not* a toggle itself.</summary>
        public bool IsPremium { get; set; }
    }
}
    