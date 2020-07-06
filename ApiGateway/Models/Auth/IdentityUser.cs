using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGateway.Models.Auth
{
    public class IdentityUser
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Cell { get; set; }
        public string Username { get; set; }
    }

    public class AuthUser
    {
        public string PasswordHash { get; set; }
        public List<IdentityUserClaim> Claims { get; set; }
        public string UserName { get; set; }
        public string Id { get; set; }
        public string Email { get; set; }
    }

    public class IdentityUserClaim
    {
        public string Value
        {
            get; set;
        }

        public string Type
        {
            get; set;
        }
    }
}
