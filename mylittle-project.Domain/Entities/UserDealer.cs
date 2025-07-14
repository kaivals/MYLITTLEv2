using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Domain.Entities
{
    public class UserDealer : BaseEntity
    {

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, ErrorMessage = "Username cannot be longer than 50 characters.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required.")]
        [StringLength(20, ErrorMessage = "Role cannot be longer than 20 characters.")]
        public string Role { get; set; } = "Dealer";

        public bool IsActive { get; set; }

        [Required(ErrorMessage = "Dealer Id is required.")]
        public Guid DealerId { get; set; }

        public Dealer? Dealer { get; set; }  // ✅ Correct One-to-Many navigation

        public ICollection<TenantPortalLink>? PortalLinks { get; set; } = new List<TenantPortalLink>();
        public ICollection<PortalAssignment> PortalAssignments { get; set; } = new List<PortalAssignment>();
        public string? Name { get; set; }
    }
}
