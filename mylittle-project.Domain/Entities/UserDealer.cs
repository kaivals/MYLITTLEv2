using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Domain.Entities
{
    public class UserDealer
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, ErrorMessage = "Username cannot be longer than 50 characters.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required.")]
        [StringLength(20, ErrorMessage = "Role cannot be longer than 20 characters.")]
        public string Role { get; set; } = "Dealer";

        public bool IsActive { get; set; }

        // Foreign Key to BusinessInfo (renamed)
        [Required(ErrorMessage = "BusinessId is required.")]
        public Guid BusinessId { get; set; }

        // Navigation properties (to related entities)
        public Dealer? BusinessInfo { get; set; }

        // Collections (initialized to prevent null reference exceptions)
        public ICollection<Dealer>? BusinessInfos { get; set; } = new List<Dealer>();
        public ICollection<TenentPortalLink>? PortalLinks { get; set; } = new List<TenentPortalLink>();
        public ICollection<PortalAssignment> PortalAssignments { get; set; } = new List<PortalAssignment>();
    }
}
