using System.ComponentModel.DataAnnotations;

namespace AuthService.Model
{
    public class PasswordResetTokenDto
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string NewPassword { get; set; }
    }
}
