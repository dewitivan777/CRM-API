using ApiGateway.Models.Account;
using Client;
using Client.Models;
using IdentityModel;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using static ApiGateway.ApiConstants;

namespace ApiGateway.Auth
{
    public class DefaultGatewayAuthenticationHandler : AuthenticationHandler<DefaultGatewayAuthenticationOptions>
    {
        private readonly IApiClient _apiClient;
        private readonly IHttpContextAccessor _contextAccessor;

        private static readonly ConcurrentDictionary<string, IdentityServer4.Models.Client> Clients
            = new ConcurrentDictionary<string, IdentityServer4.Models.Client>();
        private static readonly ConcurrentDictionary<string, DateTime> ClientDates
            = new ConcurrentDictionary<string, DateTime>();


        public DefaultGatewayAuthenticationHandler(
            IOptionsMonitor<DefaultGatewayAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IHttpContextAccessor contextAccessor,
            IApiClient apiClient
            ) : base(options, logger, encoder, clock)
        {
            _contextAccessor = contextAccessor;
            _apiClient = apiClient;
        }

        protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var prop = new AuthenticationProperties();

            var header = _contextAccessor.HttpContext.Request.Headers["Token"];

            if (header.Count == 0)
            {
                // Apparently multiple default schemes were dropped moving from 1.1 to 3.1 
                var result = await _contextAccessor.HttpContext.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);

                return result;
            }

            var token = header.ToArray()[0];

            var client = await FindClientByIdAsync(token);

            if (client == null)
            {
                return AuthenticateResult.Fail("Invalid Client");
            }

            var claims = new List<Claim>();

            List<string> moderatorClaims = new List<string> { "moderator", "callcentre", "sales", "admin", "webmaster" };

            foreach (var claim in client.Claims)
            {
                if (moderatorClaims.Contains(claim.Value))
                {
                    claims.Add(new Claim(ClaimTypes.Role, "moderator"));
                }
                else if ((claim.Type == JwtClaimTypes.Role || claim.Type == JwtClaimTypes.Subject) && claim.Value == "root")
                {
                    claims.Add(new Claim(ClaimTypes.Role, "root"));
                }
                else if (claim.Type == JwtClaimTypes.Subject && claim.Value != client.ClientId)
                {
                    claims.Add(new Claim(ClaimTypes.Role, claim.Value));
                }
                else if (claim.Type == JwtClaimTypes.Subject)
                {
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, client.ClientId));
                }
                else if (claim.Type == JwtClaimTypes.PreferredUserName)
                {
                    claims.Add(new Claim(JwtClaimTypes.PreferredUserName, claim.Value));
                }
                else if (claim.Type == JwtClaimTypes.Email)
                {
                    claims.Add(new Claim(ClaimTypes.Email, claim.Value));
                }
            }

            var principal = new ClaimsPrincipal();

            principal.AddIdentity(new ClaimsIdentity(claims, Options.AuthenticationType));

            var ticket = new AuthenticationTicket(principal, prop, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }

        private async Task<IdentityServer4.Models.Client> FindClientByIdAsync(string clientId)
        {
            if (clientId.Length < 2)
                return null;

            if (ClientDates.ContainsKey(clientId))
            {
                if (ClientDates[clientId] > DateTime.Now.AddHours(-1))
                {
                    if (Clients.TryGetValue(clientId, out var cacheClient))
                        return cacheClient;
                }
            }

            var apiInfo = new ApiInfo("account", method: "query");

            string pathWithQuery = $"/service/account/?token={clientId}";

            IApiResponse<AuthUser> response = await _apiClient.GetAsync<AuthUser>(apiInfo, pathWithQuery);

            if (response.IsError || response.Content == null)
            {
                return null;
            }

            var client = response.Content;

            var userClaims = new List<Claim>();

            foreach (var claim in client.Claims)
            {
                if (claim.Type == JwtClaimTypes.Role)
                {
                    userClaims.Add(new Claim(JwtClaimTypes.Role, claim.Value));
                }
                else if (claim.Type == JwtClaimTypes.Subject && claim.Value != client.Id)
                {
                    userClaims.Add(new Claim(JwtClaimTypes.Role, claim.Value));
                }
                else if (claim.Type != JwtClaimTypes.Subject)
                {
                    userClaims.Add(new Claim(claim.Type, claim.Value));
                }
            }

            userClaims.Add(new Claim(JwtClaimTypes.Subject, client.Id));
            userClaims.Add(new Claim(JwtClaimTypes.PreferredUserName, client.UserName));
            userClaims.Add(new Claim(JwtClaimTypes.Email, client.Email));

            IdentityServer4.Models.Client user = new IdentityServer4.Models.Client
            {
                ClientId = client.UserName,
                ClientSecrets = new List<Secret>() { new Secret() { Value = client.PasswordHash } },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                RequireClientSecret = false,
                AllowedScopes = { "account" },
                Claims = userClaims
            };

            IApiResponse<ApiUserModel> userResponse = await _apiClient.GetAsync<ApiUserModel>(new ApiInfo("user", "query"), $"{Backends.User}/{client.Id}", userClaim: client.Id);

            if (userResponse.IsError || userResponse.Content == null)
                return null;

            var package = userResponse.Content.Package;
            var source = userResponse.Content.ReportingSource;
            var group = userResponse.Content.Group;

            if (!string.IsNullOrWhiteSpace(source))
            {
                user.Claims.Add(new Claim("source", source));
            }

            if (!string.IsNullOrWhiteSpace(group))
            {
                user.Claims.Add(new Claim("group", group));
            }

            if (!string.IsNullOrWhiteSpace(package))
            {
                user.Claims.Add(new Claim("package", package));
            }

            Clients.TryAdd(clientId, user);
            ClientDates.TryAdd(clientId, DateTime.Now);

            foreach (var pair in ClientDates)
            {
                if (pair.Value < DateTime.Now.AddHours(-1))
                {
                    Clients.TryRemove(pair.Key, out _);
                    ClientDates.TryRemove(pair.Key, out _);
                }
            }

            return user;
        }
    }
}
