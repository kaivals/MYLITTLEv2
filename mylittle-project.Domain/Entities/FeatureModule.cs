using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Domain.Entities
{
    /// <summary>High-level module that can group many child features.</summary>
    public class FeatureModule
    {
        public Guid Id { get; set; }

        [Required]                   // unique key for code look-ups, seed scripts, etc.
        public string Key { get; set; } = default!;

        [Required]
        public string Name { get; set; } = default!;

        public string? Description { get; set; }

        // Navigation – one module => many features
        public ICollection<Feature> Features { get; set; } = new List<Feature>();
    }
}
