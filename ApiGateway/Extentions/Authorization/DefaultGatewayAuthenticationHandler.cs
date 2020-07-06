using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using ApiGateway.Models;
using ApiGateway.Models.Auth;
using AspNetCore.ApiGateway;
using IdentityModel;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApiGateway.Extentions.Authorization
{
    public class DefaultGatewayAuthenticationHandler : AuthenticationHandler<DefaultGatewayAuthenticationOptions>
    {
        private readonly IHttpContextAccessor _contextAccessor;
        readonly IHttpService _httpService;
        readonly IApiOrchestrator _apiOrchestrator;

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            IgnoreNullValues = true,
            PropertyNameCaseInsensitive = true
        };


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
            IHttpService httpService,
            IApiOrchestrator apiOrchestrator
            ) : base(options, logger, encoder, clock)
        {
            _contextAccessor = contextAccessor;
            _httpService = httpService;
            _apiOrchestrator = apiOrchestrator;
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

            var apiInfo = _apiOrchestrator.GetApi("authservice");
            var gwRouteInfo = apiInfo.Mediator.GetRoute("account" + GatewayVerb.GET);
            var routeInfo = gwRouteInfo.Route;

            var identityUser = new AuthUser();

            using (var client = routeInfo.HttpClientConfig?.HttpClient())
            {

                //Get IdentityUser
                var response =
                    await (client ?? _httpService.Client).GetAsync(
                        $"{apiInfo.BaseUrl}{routeInfo.Path}");

                if (response.IsSuccessStatusCode)
                {
                    identityUser = JsonSerializer.Deserialize<AuthUser>(await response.Content.ReadAsStringAsync(), _jsonSerializerOptions);
                }

                if (!response.IsSuccessStatusCode || response.Content == null)
                {
                    return null;
                }
            }

            var userClaims = new List<ClientClaim>();

            foreach (var claim in identityUser.Claims)
            {
                if (claim.Type == JwtClaimTypes.Role)
                {
                    userClaims.Add(new ClientClaim(JwtClaimTypes.Role, claim.Value));
                }
                else if (claim.Type == JwtClaimTypes.Subject && claim.Value != identityUser.Id)
                {
                    userClaims.Add(new ClientClaim(JwtClaimTypes.Role, claim.Value));
                }
                else if (claim.Type != JwtClaimTypes.Subject)
                {
                    userClaims.Add(new ClientClaim(claim.Type, claim.Value));
                }
            }

            userClaims.Add(new ClientClaim(JwtClaimTypes.Subject, identityUser.Id));
            userClaims.Add(new ClientClaim(JwtClaimTypes.PreferredUserName, identityUser.UserName));
            userClaims.Add(new ClientClaim(JwtClaimTypes.Email, identityUser.Email));

            IdentityServer4.Models.Client user = new IdentityServer4.Models.Client
            {
                ClientId = identityUser.UserName,
                ClientSecrets = new List<Secret>() { new Secret() { Value = identityUser.PasswordHash } },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                RequireClientSecret = false,
                AllowedScopes = { "account" },
                Claims = userClaims
            };

             apiInfo = _apiOrchestrator.GetApi("userservice");
             gwRouteInfo = apiInfo.Mediator.GetRoute("user" + GatewayVerb.GET);
             routeInfo = gwRouteInfo.Route;

             var apiUser = new ApiUserModel();

             using (var client = routeInfo.HttpClientConfig?.HttpClient())
             {

                 //Get IdentityUser
                 var response =
                     await (client ?? _httpService.Client).GetAsync(
                         $"{apiInfo.BaseUrl}{routeInfo.Path}");

                 if (response.IsSuccessStatusCode)
                 {
                     apiUser = JsonSerializer.Deserialize<ApiUserModel>(await response.Content.ReadAsStringAsync(), _jsonSerializerOptions);
                 }

                 if (!response.IsSuccessStatusCode || response.Content == null)
                 {
                     return null;
                 }
             }


            var business = apiUser.BusinessName;
            var role = apiUser.Role;

            if (!string.IsNullOrWhiteSpace(business))
            {
                user.Claims.Add(new ClientClaim("business", business));
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                user.Claims.Add(new ClientClaim("role", role));
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
