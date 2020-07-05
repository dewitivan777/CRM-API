using System.Threading.Tasks;
using AuthService.Models;

namespace AuthService.Services
{
    public interface IUserManager
    {

        Task<IdentityResult> CreateAsync(IdentityUser user, string password);

        Task SetPhoneNumberAsync(IdentityUser user, string cell);

        Task<IdentityUser> FindByNameAsync(string username);

        Task<IdentityResult> ResetPasswordAsync(IdentityUser user, string newPassword);

        Task<IdentityResult> ChangePasswordAsync(IdentityUser user, string currentPassword, string newPassword);

        Task<IdentityUser> FindByIdAsync(string userId);

        Task RemoveClaimAsync(IdentityUser user, UserClaim claim);

        Task AddClaimAsync(IdentityUser user, UserClaim claim);

        Task<IdentityResult> ChangeEmailAsync(IdentityUser user, string newEmail);

        Task<IdentityResult> SetUserNameAsync(IdentityUser user, string userName);
    }
}
