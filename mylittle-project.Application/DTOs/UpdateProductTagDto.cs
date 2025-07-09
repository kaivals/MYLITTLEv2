using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Application.DTOs
{
    public class UpdateProductTagDto
    {
        [Required(ErrorMessage = "Tag name is required.")]
        [MaxLength(100, ErrorMessage = "Tag name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        public bool Published { get; set; }
    }
}
