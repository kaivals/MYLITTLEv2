using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Application.DTOs
{
    public class ProductCreateDto
    {
        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = string.Empty;
    }
}
