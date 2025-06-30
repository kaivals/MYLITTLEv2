using System;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Domain.Entities
{
    public class KycDocumentUpload
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "BusinessInfoId is required.")]
        public Guid BusinessInfoId { get; set; }

        [Required(ErrorMessage = "Document type is required.")]
        [StringLength(100, ErrorMessage = "DocType cannot exceed 100 characters.")]
        public string DocType { get; set; } = string.Empty;

        [Required(ErrorMessage = "File URL is required.")]
        [StringLength(300, ErrorMessage = "FileUrl cannot exceed 300 characters.")]
        public string FileUrl { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public Dealer? BusinessInfo { get; set; }
    }
}
