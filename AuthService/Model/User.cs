using System;
using System.Collections.Generic;
using System.Linq;
using AuthService.Model;
using AuthService.Services;

namespace IdentityMicroservice.Model
{
    public class User 
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public List<UserClaim> Claims { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime CreatedOn { get; set; }

        public void SetPassword(string password, IEncryptor encryptor)
        {
            Salt = encryptor.GetSalt(password);
            Password = encryptor.GetHash(password, Salt);
        }

        public bool ValidatePassword(string password, IEncryptor encryptor)
        {
            var isValid = Password.Equals(encryptor.GetHash(password, Salt));
            return isValid;
        }

        public virtual void AddClaim(UserClaim claim)
        {
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            Claims.Add(claim);
        }

        public virtual void RemoveClaim(UserClaim claim)
        {
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            var items = Claims.Where(tbl => tbl.Type == claim.Type && tbl.Value == claim.Value).ToList();

            foreach (var item in items)
            {
                Claims.Remove(item);
            }
        }
    }
}
