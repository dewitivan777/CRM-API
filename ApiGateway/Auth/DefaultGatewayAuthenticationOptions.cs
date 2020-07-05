using Microsoft.AspNetCore.Authentication;

namespace ApiGateway.Auth
{
    public class DefaultGatewayAuthenticationOptions : AuthenticationSchemeOptions
    {
        public string AuthenticationType { get; set; }
    }
}
