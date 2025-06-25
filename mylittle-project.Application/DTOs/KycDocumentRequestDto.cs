using System;
using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Application.DTOs
{
    public class KycDocumentRequestDto
    {
        [Required(ErrorMessage = "BusinessInfoId is required.")]
        public Guid BusinessInfoId { get; set; }

        [Required(ErrorMessage = "Document type is required.")]
        [MaxLength(100, ErrorMessage = "Document type cannot exceed 100 characters.")]
        public string DocType { get; set; } = string.Empty;

        public bool IsRequired { get; set; }
    }
}
