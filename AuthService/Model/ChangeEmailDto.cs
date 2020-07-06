using System.ComponentModel.DataAnnotations;
using Services.Domain.Auth.Models;

namespace AuthService.Model
{
    public class ChangeEmailDto : BaseEntity
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
