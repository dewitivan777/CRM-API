using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using ApiGateway.Models;
using ApiGateway.Models.Auth;
using AspNetCore.ApiGateway;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace ApiGateway.Extentions.Authorization
{
    public class GatewayResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly IHttpService _httpService;
        private readonly ISystemClock _clock;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPasswordHasher<string> _resourceOwnerPasswordHasher;
        private readonly IApiOrchestrator _apiOrchestrator;

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            IgnoreNullValues = true,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Initialize a new instance of <see cref="GatewayResourceOwnerPasswordValidator"/>
        /// </summary>
        /// <param name="httpService"></param>
        /// <param name="clock"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="hasher"></param>
        public GatewayResourceOwnerPasswordValidator(
            IHttpService httpService,
            ISystemClock clock,
            IHttpContextAccessor httpContextAccessor,
            IPasswordHasher<string> hasher,
            IApiOrchestrator apiOrchestrator)
        {
            _httpService = httpService;
            _clock = clock;
            _httpContextAccessor = httpContextAccessor;
            _resourceOwnerPasswordHasher = hasher;
            _apiOrchestrator = apiOrchestrator;
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

            var apiInfo = _apiOrchestrator.GetApi("authservice");
            var gwRouteInfo = apiInfo.Mediator.GetRoute("account" + GatewayVerb.GET);
            var routeInfo = gwRouteInfo.Route;

            // pathWithQuery = $"/service/account?username={userName}";

            //bool requiresSecret = true;

            //Not email and not phone number
            if (!userName.Contains('@') && userName.Length > 15)
            {
                //requiresSecret = true;
               // pathWithQuery = $"/service/account?userId={userName}";
            }

            var identityUser = new AuthUser();

            using (var client = routeInfo.HttpClientConfig?.HttpClient())
            {

                //Get IdentityUser
                var response =
                    await (client ?? _httpService.Client).GetAsync(
                        $"{apiInfo.BaseUrl}{routeInfo.Path}");

                if (!response.IsSuccessStatusCode || response.Content == null)
                {
                    fail.ErrorDescription = "User Not Found";
                    fail.Error = "User Not Found";
                    context.Result = fail;
                    return;
                }

                if (response.IsSuccessStatusCode)
                {
                    identityUser = JsonSerializer.Deserialize<AuthUser>(await response.Content.ReadAsStringAsync(), _jsonSerializerOptions);
                }
            }

            //Simplification of moderator rights. All of these claims will be given the 'moderator' claim
            //Even though not secure, it only applies to moderators
            List<string> moderatorClaims = new List<string> { "moderator", "callcentre", "sales", "admin", "webmaster" };

            var userClaims = new List<Claim>();

            if (identityUser.Claims.Any(claim => (claim.Type == JwtClaimTypes.Role || claim.Type == JwtClaimTypes.Subject)
                                                 && moderatorClaims.Contains(claim.Value)))
            {
                //userClaims.Add(new Claim("sub", "moderator"));
                userClaims.Add(new Claim(JwtClaimTypes.Role, "moderator"));
            }

            if (identityUser.Claims.Any(claim => (claim.Type == JwtClaimTypes.Role || claim.Type == JwtClaimTypes.Subject)
                                                 && claim.Value == "root"))
            {
                userClaims.Add(new Claim(JwtClaimTypes.Role, "root"));
            }

            userClaims.Add(new Claim(JwtClaimTypes.PreferredUserName, identityUser.UserName));
            userClaims.Add(new Claim(ClaimTypes.Email, identityUser.Email));

            //Check Password
            var passwordValidationResult = _resourceOwnerPasswordHasher.VerifyHashedPassword(identityUser.Id, identityUser.PasswordHash, context.Password);

            if (passwordValidationResult == PasswordVerificationResult.Failed)
            {
                fail.ErrorDescription = "Password invalid";
                context.Result = fail;
                return;
            }


            //Set routeInfo
            apiInfo = _apiOrchestrator.GetApi("UserService");
            gwRouteInfo = apiInfo.Mediator.GetRoute("User" + GatewayVerb.GET);
            routeInfo = gwRouteInfo.Route;

            var apiUser = new ApiUserModel();

            //Get User 
            using (var client = routeInfo.HttpClientConfig?.HttpClient())
            {

                //Get IdentityUser
                var response =
                    await (client ?? _httpService.Client).GetAsync(
                        $"{apiInfo.BaseUrl}{routeInfo.Path}");

                // Only active accounts may log in
                if (!response.IsSuccessStatusCode
                    || response.Content == null)
                {
                    fail.ErrorDescription = "User Not Active";
                    fail.Error = "User Not Active";
                    context.Result = fail;
                    return;
                }

                if (response.IsSuccessStatusCode)
                {
                    apiUser = JsonSerializer.Deserialize<ApiUserModel>(await response.Content.ReadAsStringAsync());
                }

            }

            if (apiUser.State != "Active")
            {
                fail.ErrorDescription = "User Not Active";
                fail.Error = "User Not Active";
                context.Result = fail;
                return;
            }


            var business = apiUser.BusinessName;
            var role = apiUser.Role;

            if (!string.IsNullOrWhiteSpace(business))
            {
                userClaims.Add(new Claim("business", business));
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                userClaims.Add(new Claim("role", role));
            }

            var authTime = _clock.UtcNow
                .LocalDateTime
               .ToEpochTime()
               .ToString();

            userClaims.Add(new Claim(JwtClaimTypes.AuthenticationTime, authTime));

            var result = new GrantValidationResult(
                apiUser.Id,
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
