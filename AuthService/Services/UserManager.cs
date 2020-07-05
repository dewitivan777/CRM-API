using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;


namespace AuthService.Services
{
    public class UserManager : IUserManager
    {
        private readonly DbContextOptions<SqlDbContext> _options;
        private readonly Microsoft.AspNetCore.Identity.IPasswordHasher<string> _hasher;

        public UserManager(DbContextOptions<SqlDbContext> options, Microsoft.AspNetCore.Identity.IPasswordHasher<string> hasher)
        {
            _options = options;
            _hasher = hasher;
        }

        public async Task<IdentityResult> CreateAsync(IdentityUser user, string password)
        {
            password = password.Trim();

            if (string.IsNullOrEmpty(password) || password.Length < 7)
            {
                return new IdentityResult()
                {
                    Errors = new List<string> { "Password must be at least 7 characters" },
                    Succeeded = false
                };
            }

            using (var db = new SqlDbContext(_options))
            {
                try
                {
                    var dbUser = db.IdentityUser.FirstOrDefault(tbl => tbl.Id == user.Id);

                    if (dbUser == null)
                    {
                        user.CreatedOn = DateTime.Now;
                        // Normalize to lower to match the current user search. Or should we keep a copy of the original?
                        user.Email = user.Email.ToLower();
                        user.UserName = user.UserName.ToLower();

                        user.PasswordHash = _hasher.HashPassword(user.Id, password);

                        db.Add(user);
                    }
                    await db.SaveChangesAsync();
                }
                catch (Exception exc)
                {

                }

                return new IdentityResult()
                {
                    Succeeded = true
                };

            }
        }

        public async Task SetPhoneNumberAsync(IdentityUser user, string cell)
        {
            using (var db = new SqlDbContext(_options))
            {
                var entity = db.IdentityUser.FirstOrDefault(tbl => tbl.Id == user.Id);

                if (entity != null)
                {
                    user.PhoneNumber = cell;
                    db.Update(entity);
                }
                await db.SaveChangesAsync();
            }
            
        }

        public async Task<IdentityUser> FindByNameAsync(string username)
        {
            return await FindOneByFieldAsync("userName", username);
        }

        private async Task<IdentityUser> FindOneByFieldAsync(string field, string value)
        {

            using (var db = new SqlDbContext(_options))
            {
                try
                {
                    var entity = db.IdentityUser.FirstOrDefault(tbl => tbl.Id == value);

                    if (entity != null)
                    {
                        return entity;
                    }

                }
                catch (Exception ex)
                {

                }

            }

            return null;
        }

        public async Task<IdentityResult> ResetPasswordAsync(IdentityUser user, string newPassword)
        {
            newPassword = newPassword.Trim();

            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 7)
            {
                return new IdentityResult()
                {
                    Errors = new List<string> { "Password must be at least 7 characters" },
                    Succeeded = false
                };
            }

            using (var db = new SqlDbContext(_options))
            {
                try
                {
                    var dbUser = db.IdentityUser.FirstOrDefault(tbl => tbl.Id == user.Id);

                    if (dbUser == null)
                    {
                        user.PasswordHash = _hasher.HashPassword(user.Id, newPassword);

                        db.Update(user);
                    }
                    await db.SaveChangesAsync();
                }
                catch (Exception exc)
                {

                }

                return new IdentityResult()
                {
                    Succeeded = true
                };

            }

        }

        public async Task<IdentityResult> ChangePasswordAsync(IdentityUser user, string currentPassword, string newPassword)
        {
            var verifyResult = _hasher.VerifyHashedPassword(user.Id, user.PasswordHash, currentPassword);

            if (verifyResult != Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success)
            {
                return new IdentityResult()
                {
                    Succeeded = false,
                    Errors = new List<string>() { "Incorrect password supplied" }
                };
            }

            newPassword = newPassword.Trim();

            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 7)
            {
                return new IdentityResult()
                {
                    Errors = new List<string> { "Password must be at least 7 characters" },
                    Succeeded = false
                };
            }


            using (var db = new SqlDbContext(_options))
            {
                try
                {
                    var dbUser = db.IdentityUser.FirstOrDefault(tbl => tbl.Id == user.Id);

                    if (dbUser == null)
                    {
                        user.PasswordHash = _hasher.HashPassword(user.Id, newPassword);

                        db.Update(user);
                    }
                    await db.SaveChangesAsync();
                }
                catch (Exception exc)
                {

                }

                return new IdentityResult()
                {
                    Succeeded = true
                };

            }

        }

        public async Task<IdentityUser> FindByIdAsync(string userId)
        {
           // return await _indexHandler.GetEntityByIdAsync(userId);

           return  new IdentityUser();
        }

        public async Task RemoveClaimAsync(IdentityUser user, Models.UserClaim claim)
        {
         

            using (var db = new SqlDbContext(_options))
            {
                try
                {
                    var dbUser = db.IdentityUser.FirstOrDefault(tbl => tbl.Id == user.Id);
                 
                    if (dbUser == null)
                    {
                        user.RemoveClaim(claim);
                        db.Update(user);
                    }
                    await db.SaveChangesAsync();
                }
                catch (Exception exc)
                {

                }
            }
        }

        public async Task AddClaimAsync(IdentityUser user, Models.UserClaim claim)
        {
            using (var db = new SqlDbContext(_options))
            {
                try
                {
                    var dbUser = db.IdentityUser.FirstOrDefault(tbl => tbl.Id == user.Id);

                    if (dbUser == null)
                    {
                        user.AddClaim(claim);
                        db.Update(user);
                    }
                    await db.SaveChangesAsync();
                }
                catch (Exception exc)
                {

                }
            }
        }

        public async Task<IdentityResult> ChangeEmailAsync(IdentityUser user, string newEmail)
        {
            using (var db = new SqlDbContext(_options))
            {
                try
                {
                    var dbUser = db.IdentityUser.FirstOrDefault(tbl => tbl.Email == newEmail);

                    if (dbUser == null)
                    {
                        user.Email = newEmail;
                        db.Update(user);
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        return new IdentityResult()
                        {
                            Errors = new List<string> { "Email already taken" },
                            Succeeded = false
                        };
                    }

                
                }
                catch (Exception exc)
                {

                }
            }


            return new IdentityResult
            {
                Succeeded = true
            };
        }


        

        public async Task<IdentityResult> SetUserNameAsync(IdentityUser user, string userName)
        {
            user.Email = user.UserName.ToLower();



            //await _indexHandler.UpdateAsync(user);

            return new IdentityResult
            {
                Succeeded = true
            };
        }
    }
}
