using System;
using AuthService.Models;

namespace OAuthService.Models
{
    public class PasswordResetToken : BaseEntityToken
    {
        public string Email { get; set; }
        public DateTime RequestedOn { get; set; }
    }
}
