using System.Collections.Generic;
using System.Linq;
using ApiGateway.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace ApiGateway.Extentions
{
    /// <summary>
    /// IIdentityServerBuilder extension methods for registering custom gateway services
    /// </summary>
    public static class IdentityServerBuilderExtensionsGateway
    {
        /// <summary>
        /// Add the gateway custom client store
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="inMemoryClients"></param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddGatewayClientStore(this IIdentityServerBuilder builder, IEnumerable<IdentityServer4.Models.Client> inMemoryClients = null)
        {
            builder.Services.AddSingleton(inMemoryClients ?? Enumerable.Empty<IdentityServer4.Models.Client>());

            builder.AddClientStore<GatewayClientStore>();

            return builder;
        }
    }
}
