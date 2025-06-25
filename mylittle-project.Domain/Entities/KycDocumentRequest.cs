using System;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Domain.Entities
{
    public class KycDocumentRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "BusinessInfoId is required.")]
        public Guid BusinessInfoId { get; set; }

        [Required(ErrorMessage = "Document type is required.")]
        [StringLength(50, ErrorMessage = "Document type can't exceed 50 characters.")]
        public string DocType { get; set; } = string.Empty;

        public bool IsRequired { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
