using Microsoft.AspNetCore.Authentication;

namespace ApiGateway.Extentions.Authorization
{
    public class DefaultGatewayAuthenticationOptions : AuthenticationSchemeOptions
    {
        public string AuthenticationType { get; set; }
    }
}
