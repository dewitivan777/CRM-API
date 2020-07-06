using System;

namespace AuthService.Model
{
    public class PasswordResetToken : BaseEntityToken
    {
        public string Email { get; set; }
        public DateTime RequestedOn { get; set; }
    }
}
