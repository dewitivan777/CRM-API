using System.ComponentModel.DataAnnotations;

namespace AuthService.Model
{
    public class ChangePasswordDto
    {
        [Required]
        public string Username { get; set; }

        public string CurrentPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }
    }
}
