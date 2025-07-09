using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Application.DTOs
{
    public class CreateProductTagDto
    {
        [Required(ErrorMessage = "Tag name is required.")]
        [StringLength(100, ErrorMessage = "Tag name cannot exceed 100 characters.")]
        public string Name { get; set; } = null!;

        public bool Published { get; set; } = true;
    }
}
