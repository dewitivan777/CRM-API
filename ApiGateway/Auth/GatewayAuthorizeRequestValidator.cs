using IdentityServer4.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGateway.Auth
{
    public class GatewayAuthorizeRequestValidator : ICustomAuthorizeRequestValidator
    {
        public async Task ValidateAsync(CustomAuthorizeRequestValidationContext context)
        {
            context.Result.IsError = false;
            await Task.CompletedTask;
        }
    }
}
