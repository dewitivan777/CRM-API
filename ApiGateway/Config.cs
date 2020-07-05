using System.Collections.Generic;
using IdentityServer4.Models;

namespace ApiGateway
{
    public class Config
    {
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("account","account service")
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Email(),
                new IdentityResources.Phone()
            };
        }

        /// <summary>
        /// Allowed clients. In memory for now, we can always persist them if the need arises.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IdentityServer4.Models.Client> GetClients()
        {
            // client credentials client
            return new List<IdentityServer4.Models.Client>
            {
                new IdentityServer4.Models.Client
                {
                    ClientId = "jma.web",
                    ClientName = "web",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                    ClientSecrets =
                    {
                        new Secret("web@jma.client".Sha256())
                    },
                    AllowedScopes = { "account" },
                    AccessTokenLifetime = 604800
                },
                new IdentityServer4.Models.Client
                {
                    ClientId = "jma.accounts",
                    ClientName = "accounts",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                    ClientSecrets =
                    {
                        new Secret("accounts@jma.client".Sha256())
                    },
                    AllowedScopes = { "account" },
                    AccessTokenLifetime = 604800
                },
                new IdentityServer4.Models.Client
                {
                    ClientId = "jma.admin",
                    ClientName = "admin",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                    ClientSecrets =
                    {
                        new Secret("admin@jma.client".Sha256())
                    },
                    AllowedScopes = { "account" },
                    AccessTokenLifetime = 604800
                },
                new IdentityServer4.Models.Client
                {
                    ClientId = "jma.android",
                    ClientName = "android",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                    ClientSecrets =
                    {
                        new Secret("android@jma.client".Sha256())
                    },
                    AllowedScopes = { "account" },
                    AccessTokenLifetime = 604800
                }
            };
        }
    }
}
