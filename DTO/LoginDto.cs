using System.ComponentModel.DataAnnotations;

namespace TaskTrackingApi.Dtos
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(8, ErrorMessage = "Password must be 8 characters or less")]
        public string Password { get; set; } = string.Empty;
    }
}
