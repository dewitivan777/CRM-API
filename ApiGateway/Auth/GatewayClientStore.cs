using ApiGateway.Extentions;
using ApiGateway.Models.Account;
using Client;
using Client.Models;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static ApiGateway.ApiConstants;

namespace ApiGateway.Auth
{
    /// <summary>
    /// 
    /// </summary>
    public class GatewayClientStore : IClientStore
    {
        private readonly IApiClient _apiClient;
        private readonly IEnumerable<IdentityServer4.Models.Client> _inMemoryClients;

        /// <summary>
        /// Initialize a new instance of <see cref="GatewayClientStore"/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="inMemoryClients"></param>
        public GatewayClientStore(IApiClient client, IEnumerable<IdentityServer4.Models.Client> inMemoryClients)
        {
            if (inMemoryClients.ContainsDuplicates(m => m.ClientId))
            {
                throw new ArgumentException("Clients must not contain duplicate ids");
            }

            _inMemoryClients = inMemoryClients;
            _apiClient = client;
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

            var apiInfo = new ApiInfo("account", method: "query");

            var pathWithQuery = $"/service/account?username={clientId}";

            bool requiresSecret = true;

            //Not email and not phone number
            if (!clientId.Contains('@') && clientId.Length > 15)
            {
                requiresSecret = true;
                pathWithQuery = $"/service/account?userId={clientId}";
            }

            IApiResponse<AuthUser> response = await _apiClient.GetAsync<AuthUser>(apiInfo, pathWithQuery);

            if (response.IsError || response.Content == null)
            {
                return null;
            }

            var client = response.Content;

            //Simplification of moderator rights. All of these claims will be given the 'moderator' claim
            //Even though not secure, it only applies to moderators
            List<string> moderatorClaims = new List<string> { "moderator", "callcentre", "sales", "admin", "webmaster" };

            var userClaims = new List<Claim>();

            // sub claim should be reserved for userId to conform with industry standards
            // roles currently persisted as sub claims
            // here assigning them to role claims in principal
            if (client.Claims.Any(tbl => (tbl.Type == JwtClaimTypes.Role || tbl.Type == JwtClaimTypes.Subject) && moderatorClaims.Contains(tbl.Value)))
            {
                userClaims.Add(new Claim(JwtClaimTypes.Role, "moderator"));
            }

            if (client.Claims.Any(tbl => (tbl.Type == JwtClaimTypes.Role || tbl.Type == JwtClaimTypes.Subject) && tbl.Value == "root"))
            {
                userClaims.Add(new Claim(JwtClaimTypes.Role, "root"));
            }

            var user = new IdentityServer4.Models.Client
            {
                ClientId = client.UserName,
                ClientSecrets = new List<Secret>() { new Secret() { Value = client.PasswordHash } },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                RequireClientSecret = requiresSecret,
                AllowedScopes = { "account" },
                Claims = userClaims,
                AccessTokenLifetime = 604800,
                ClientClaimsPrefix = string.Empty
            };

            // sub claim is assigned userId instead
            user.Claims.Add(new Claim(JwtClaimTypes.Subject, client.Id));

            user.Claims.Add(new Claim(JwtClaimTypes.PreferredUserName, client.UserName));
            user.Claims.Add(new Claim(ClaimTypes.Email, client.Email));

            IApiResponse<ApiUserModel> userResponse = await _apiClient.GetAsync<ApiUserModel>(new ApiInfo("user", "query"), $"{Backends.User}/{client.Id}", userClaim: client.Id);

            //Only active accounts may log in
            if (userResponse.IsError || userResponse.Content == null || userResponse.Content.State != "Active")
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

            return user;
        }

        private IdentityServer4.Models.Client FindClientByIdInMemoryAsync(string clientId)
        {
            var client = _inMemoryClients.SingleOrDefault(c => c.ClientId == clientId);

            return client;
        }
    }
}
