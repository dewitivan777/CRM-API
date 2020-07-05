using IdentityServer4.Models;
using IdentityServer4.Services;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ApiGateway.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class GatewayProfileService : IProfileService
    {
        /// <summary>
        /// Get profile data
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var principal = context?.Subject;

            // claims already assigned during validation, just issue them
            var claims = principal?.Claims?.ToList();
            context.IssuedClaims = claims ?? new List<Claim>();

            return Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task IsActiveAsync(IsActiveContext context)
        {
            // check already done during validation
            return Task.CompletedTask;
        }
    }
}
