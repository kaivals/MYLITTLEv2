using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(50, ErrorMessage = "First Name cannot exceed 50 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters.")]
        public string LastName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public override string Email
        {
            get => base.Email!;
            set => base.Email = value;
        }

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(256, ErrorMessage = "Username cannot exceed 256 characters.")]
        public override string UserName
        {
            get => base.UserName!;
            set => base.UserName = value;
        }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        public override string? PhoneNumber
        {
            get => base.PhoneNumber;
            set => base.PhoneNumber = value;
        }
    }
}
