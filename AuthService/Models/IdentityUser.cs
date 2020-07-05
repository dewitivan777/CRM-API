using System;
using System.Collections.Generic;
using System.Linq;
using Services.Domain.Auth.Models;

namespace AuthService.Models
{
    public class IdentityUser : BaseEntity
    {
        public IdentityUser()
        {
        }

        public IdentityUser(string userName, string email) : this(userName)
        {
            Email = email;
        }

        public IdentityUser(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentNullException(nameof(userName));
            }

            Id = Guid.NewGuid().ToString("N");
            UserName = userName;
            CreatedOn = DateTime.Now;

            EnsureClaimsIsSet();
        }

        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string PasswordHash { get; set; }
        public List<UserClaim> Claims { get; set; }
        public int AccessFailedCount { get; set; }
        public bool IsLockoutEnabled { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? DeletedOn { get; set; }

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

        public void Delete()
        {
            if (DeletedOn != null)
            {
                throw new InvalidOperationException($"User '{Id}' has already been deleted.");
            }

            DeletedOn = DateTime.Now;
        }

        private void EnsureClaimsIsSet()
        {
            if (Claims == null)
            {
                Claims = new List<UserClaim>();
            }
        }
    }
}
