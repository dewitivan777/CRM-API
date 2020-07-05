using System.ComponentModel.DataAnnotations;
using Services.Domain.Auth.Models;

namespace AuthService.Models
{
    public class ChangeEmailDto : BaseEntity
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
