using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ApiGateway.Models.Auth;
using AspNetCore.ApiGateway;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ApiGateway.Extentions.Authorization
{
    public class AuthProvider : IAuthProvider
    {
        private IHttpContextAccessor _contextAccessor;
        private readonly List<ApiScope> _scopes;

        private static readonly HashSet<string> _knownApps = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Accounts",
            "Admin",
            "Android",
            "iOS",
            "Web"
        };

        /// <summary>
        /// Initialize a new instance of <see cref="AuthProvider"/>
        /// </summary>
        /// <param name="contextAccessor"></param>
        /// <param name="scopes"></param>
        public AuthProvider(IHttpContextAccessor contextAccessor, IOptions<List<ApiScope>> scopes)
        {
            _contextAccessor = contextAccessor;
            _scopes = scopes.Value;
        }

        /// <summary>
        /// Determine API scope
        /// </summary>
        /// <param name="info"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public ApiScopeResult IsApiInScope(RouteInfo info, string path = null)
        {
            List<string> roles = new List<string>() { "any" };

            foreach (var claim in _contextAccessor.HttpContext.User.Claims)
            {
                if (claim.Type == ClaimTypes.Role)
                {
                    roles.Add(claim.Value);
                }
            }

            var source = GetSource();

            ApiScopeResult result = new ApiScopeResult();

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetUserId()
        {
            var userId = _contextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return userId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetSource()
        {
            if (!_contextAccessor.HttpContext.Request.Headers.TryGetValue("crm-source", out var source))
            {
                return null;
            }

            return source;
        }
    }
}
