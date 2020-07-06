using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace ApiGateway.Extentions.Authorization
{
    public class GatewaySecretValidator : ISecretValidator
    {
        IPasswordHasher<string> _passwordHasher;
        private readonly IHttpContextAccessor _contextAccessor;

        /// <summary>
        /// Initialize a new instance of <see cref="GatewaySecretValidator"/>
        /// </summary>
        /// <param name="hasher"></param>
        /// <param name="contextAccessor"></param>
        public GatewaySecretValidator(IPasswordHasher<string> hasher, IHttpContextAccessor contextAccessor)
        {
            _passwordHasher = hasher;
            _contextAccessor = contextAccessor;
        }

        /// <summary>
        /// Validate api secrets
        /// </summary>
        /// <param name="secrets"></param>
        /// <param name="parsedSecret"></param>
        /// <returns></returns>
        public async Task<SecretValidationResult> ValidateAsync(IEnumerable<Secret> secrets, ParsedSecret parsedSecret)
        {
            SecretValidationResult validationResult;

            // Impersonation. If the current user is an admin, we allow any password to be used for the token
            if (_contextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                if (_contextAccessor.HttpContext.User.HasClaim(tbl => tbl.Type == ClaimTypes.Role && (tbl.Value.ToLower() == "root" || tbl.Value.ToLower() == "moderator")))
                {
                    validationResult = new SecretValidationResult()
                    {
                        IsError = false,
                        Success = true
                    };

                    return await Task.FromResult(validationResult);
                }
            }

            // Owner's credentials
            var result = _passwordHasher.VerifyHashedPassword(
                parsedSecret.Id,
                secrets?.FirstOrDefault()?.Value,
                parsedSecret.Credential.ToString());

            validationResult = new SecretValidationResult
            {
                IsError = result == PasswordVerificationResult.Failed,
                Success = result == PasswordVerificationResult.Success
            };

            return await Task.FromResult(validationResult);
        }
    }
}
