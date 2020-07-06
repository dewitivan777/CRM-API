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
using IdentityServer4.Stores;

namespace ApiGateway.Extentions.Authorization.ClientStore
{
    public class GatewayClientStore : IClientStore
    {
        readonly IApiOrchestrator _apiOrchestrator;
        private readonly IEnumerable<Client> _inMemoryClients;
        readonly IHttpService _httpService;

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            IgnoreNullValues = true,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Initialize a new instance of <see cref="GatewayClientStore"/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="inMemoryClients"></param>
        public GatewayClientStore(IApiOrchestrator apiOrchestrator , IEnumerable<IdentityServer4.Models.Client> inMemoryClients, IHttpService httpService)
        {

            _inMemoryClients = inMemoryClients;
            _apiOrchestrator = apiOrchestrator;
            _httpService = httpService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public async Task<IdentityServer4.Models.Client> FindClientByIdAsync(string clientId)
        {
            if (clientId.Length < 2)
                return null;

            // For in memory trusted clients
            var inMemoryClient = FindClientByIdInMemoryAsync(clientId);

            if (inMemoryClient != null)
            {
                return inMemoryClient;
            }

            var apiInfo = _apiOrchestrator.GetApi("authservice");
            var gwRouteInfo = apiInfo.Mediator.GetRoute("account" + GatewayVerb.GET);
            var routeInfo = gwRouteInfo.Route;

            bool requiresSecret = true;

            //Not email and not phone number
            if (!clientId.Contains('@') && clientId.Length > 15)
            {
                requiresSecret = true;
                // pathWithQuery = $"/service/account?userId={userName}";
            }

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
            }


            //Simplification of moderator rights. All of these claims will be given the 'moderator' claim
            //Even though not secure, it only applies to moderators
            List<string> moderatorClaims = new List<string> { "root" };

            var userClaims = new List<ClientClaim>();

            // sub claim should be reserved for userId to conform with industry standards
            // roles currently persisted as sub claims
            // here assigning them to role claims in principal
            if (identityUser.Claims.Any(tbl => (tbl.Type == JwtClaimTypes.Role || tbl.Type == JwtClaimTypes.Subject) && tbl.Value == "root"))
            {
                userClaims.Add(new ClientClaim(JwtClaimTypes.Role, "root"));
            }

            var user = new IdentityServer4.Models.Client
            {
                ClientId = identityUser.UserName,
                ClientSecrets = new List<Secret>() { new Secret() { Value = identityUser.PasswordHash } },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                RequireClientSecret = requiresSecret,
                AllowedScopes = { "account" },
                Claims = userClaims,
                AccessTokenLifetime = 604800,
                ClientClaimsPrefix = string.Empty
            };

            // sub claim is assigned userId instead
            user.Claims.Add(new ClientClaim(JwtClaimTypes.Subject, identityUser.Id));

            user.Claims.Add(new ClientClaim(JwtClaimTypes.PreferredUserName, identityUser.UserName));
            user.Claims.Add(new ClientClaim(ClaimTypes.Email, identityUser.Email));

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

                if (response.IsSuccessStatusCode)
                {
                    apiUser = JsonSerializer.Deserialize<ApiUserModel>(await response.Content.ReadAsStringAsync());
                    //Only active accounts may log in
                    if (response.IsSuccessStatusCode || apiUser.State == "Active")
                    {
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
                    }
                }
            }

            return user;
        }

        private IdentityServer4.Models.Client FindClientByIdInMemoryAsync(string clientId)
        {
            var client = _inMemoryClients.SingleOrDefault(c => c.ClientId == clientId);

            return client;
        }
    }
}
