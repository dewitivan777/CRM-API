using System;
using System.Collections.Generic;
using System.Linq;
using ApiGateway.Extentions.Authorization.ClientStore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace ApiGateway.Extentions.Authorization
{
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

        public static AuthenticationBuilder AddGatewayAuthentication(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<DefaultGatewayAuthenticationOptions> configureOptions)
        {
            return builder.AddScheme<DefaultGatewayAuthenticationOptions, DefaultGatewayAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
        }

    }

}
