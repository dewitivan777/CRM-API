using ApiGateway.Models.Account;
using Client;
using Client.Models;
using IdentityModel;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static ApiGateway.ApiConstants;

namespace ApiGateway.Auth
{
    /// <summary>
    /// Gateway resource owner password validator
    /// </summary>
    public class GatewayResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly IApiClient _apiClient;
        private readonly ISystemClock _clock;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPasswordHasher<string> _resourceOwnerPasswordHasher;

        /// <summary>
        /// Initialize a new instance of <see cref="GatewayResourceOwnerPasswordValidator"/>
        /// </summary>
        /// <param name="apiClient"></param>
        /// <param name="clock"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="hasher"></param>
        public GatewayResourceOwnerPasswordValidator(
            IApiClient apiClient,
            ISystemClock clock,
            IHttpContextAccessor httpContextAccessor,
            IPasswordHasher<string> hasher)
        {
            _apiClient = apiClient;
            _clock = clock;
            _httpContextAccessor = httpContextAccessor;
            _resourceOwnerPasswordHasher = hasher;
        }

        /// <summary>
        /// Validate password
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            // fail result
            var fail = new GrantValidationResult
            {
                IsError = true
            };

            // Get User
            var userName = context.UserName;
            var apiInfo = new ApiInfo("account", method: "query");

            var pathWithQuery = $"/service/account?username={userName}";

            //bool requiresSecret = true;

            //Not email and not phone number
            if (!userName.Contains('@') && userName.Length > 15)
            {
                //requiresSecret = true;
                pathWithQuery = $"/service/account?userId={userName}";
            }

            var userAccountResponse = await _apiClient.GetAsync<AuthUser>(apiInfo, pathWithQuery);

            if (userAccountResponse.IsError || userAccountResponse.Content == null)
            {
                fail.ErrorDescription = "User Not Found";
                fail.Error = "User Not Found";
                context.Result = fail;
                return;
            }

            var user = userAccountResponse.Content;

            //Simplification of moderator rights. All of these claims will be given the 'moderator' claim
            //Even though not secure, it only applies to moderators
            List<string> moderatorClaims = new List<string> { "moderator", "callcentre", "sales", "admin", "webmaster" };

            var userClaims = new List<Claim>();

            if (user.Claims.Any(claim => (claim.Type == JwtClaimTypes.Role || claim.Type == JwtClaimTypes.Subject)
                && moderatorClaims.Contains(claim.Value)))
            {
                //userClaims.Add(new Claim("sub", "moderator"));
                userClaims.Add(new Claim(JwtClaimTypes.Role, "moderator"));
            }

            if (user.Claims.Any(claim => (claim.Type == JwtClaimTypes.Role || claim.Type == JwtClaimTypes.Subject)
                && claim.Value == "root"))
            {
                userClaims.Add(new Claim(JwtClaimTypes.Role, "root"));
            }

            userClaims.Add(new Claim(JwtClaimTypes.PreferredUserName, user.UserName));
            userClaims.Add(new Claim(ClaimTypes.Email, user.Email));

            //Check Password
            var passwordValidationResult = _resourceOwnerPasswordHasher.VerifyHashedPassword(user.Id, user.PasswordHash, context.Password);

            if (passwordValidationResult == PasswordVerificationResult.Failed)
            {
                fail.ErrorDescription = "Password invalid";
                context.Result = fail;
                return;
            }

            var userDetailsResponse = await _apiClient.GetAsync<ApiUserModel>(
                new ApiInfo("user", "query"),
                $"{Backends.User}/{user.Id}",
                userClaim: user.Id);

            // Only active accounts may log in
            if (userDetailsResponse.IsError
                || userDetailsResponse.Content == null
                || userDetailsResponse.Content.State != "Active")
            {
                fail.ErrorDescription = "User Not Active";
                fail.Error = "User Not Active";
                context.Result = fail;
                return;
            }

            var package = userDetailsResponse.Content.Package;
            var source = userDetailsResponse.Content.ReportingSource;
            var group = userDetailsResponse.Content.Group;

            if (!string.IsNullOrWhiteSpace(source))
            {
                userClaims.Add(new Claim("source", source));
            }

            if (!string.IsNullOrWhiteSpace(group))
            {
                userClaims.Add(new Claim("group", group));
            }

            if (!string.IsNullOrWhiteSpace(package))
            {
                userClaims.Add(new Claim("package", package));
            }

            var authTime = _clock.UtcNow
                .LocalDateTime
               .ToEpochTime()
               .ToString();

            userClaims.Add(new Claim(JwtClaimTypes.AuthenticationTime, authTime));

            var result = new GrantValidationResult(
                user.Id,
                OidcConstants.AuthenticationMethods.Password,
                userClaims);

            // Impersonation? Might not be applicable here
            if (_httpContextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated ?? false)
            {
                if (_httpContextAccessor.HttpContext.User.HasClaim(claim => claim.Type == ClaimTypes.Role
                    && (claim.Value.ToLowerInvariant() == "root" || claim.Value.ToLowerInvariant() == "moderator")))
                {
                    context.Result = result;
                    return;
                }
            }

            context.Result = result;
        }
    }
}
