using Microsoft.AspNetCore.Authentication;
using System;

namespace ApiGateway.Auth
{
    public static class GatewayAuthenticationExtensions
    {
        public static AuthenticationBuilder AddGatewayAuthentication(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<DefaultGatewayAuthenticationOptions> configureOptions)
        {
            return builder.AddScheme<DefaultGatewayAuthenticationOptions, DefaultGatewayAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
        }
    }
}
