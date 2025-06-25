using System;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Domain.Entities
{
    public class VirtualNumberAssignment
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "BusinessId is required.")]
        public Guid BusinessId { get; set; }

        [Required(ErrorMessage = "VirtualNumber is required.")]
        [StringLength(20, ErrorMessage = "VirtualNumber cannot be longer than 20 characters.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "VirtualNumber must be a 10-digit number.")]
        public string VirtualNumber { get; set; } = string.Empty;

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public BusinessInfo? BusinessInfo { get; set; }
    }
}
