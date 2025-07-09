using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Application.DTOs
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; } = string.Empty;
    }
}
