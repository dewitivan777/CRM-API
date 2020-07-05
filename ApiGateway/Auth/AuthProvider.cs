using ApiGateway.Models;
using Client.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace ApiGateway.Auth
{
    /// <summary>
    /// 
    /// </summary>
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
        public ApiScopeResult IsApiInScope(ApiInfo info, string path = null)
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

            var isFeed = !(string.IsNullOrWhiteSpace(source) || _knownApps.Contains(source));

            //root may perform any action
            if (roles.Contains("root"))
            {
                return new ApiScopeResult()
                {
                    AllowCreateAccount = true,
                    ScopeToUser = false,
                    IsInScope = true,
                    IsModerator = true,
                    AllowRepeat = isFeed
                };
            }

            //Find all scopes related to the api info
            List<ApiScope> apiScopes = _scopes.Where(tbl =>
                (tbl.Type == info.Name
                || (tbl.Type.StartsWith("*") && info.Name.EndsWith(tbl.Type.Replace("*", "")))
                || (tbl.Type.EndsWith("*") && info.Name.StartsWith(tbl.Type.Replace("*", ""))))
                && tbl.Tags.Contains(info.Method)).ToList();

            //Find all scopes with matching sub claims
            apiScopes = apiScopes.Where(tbl => tbl.SubClaims.Any(a => roles.Contains(a))).ToList();

            //Filter/Limit scopes to the specified path
            if (string.IsNullOrEmpty(path))
            {
                apiScopes = apiScopes.Where(tbl => tbl.Paths == null).ToList();
            }
            else
            {
                var pathScopes = apiScopes.Where(tbl => tbl.Paths != null && tbl.Paths.Any(p => path.ToLower().StartsWith(p.ToLower()))).ToList();

                //Specific permissions specified for this path and should replace broarder permissions
                if (pathScopes.Count > 0)
                {
                    apiScopes = pathScopes;
                }
                else
                {
                    //Path specific scopes should not apply to other paths
                    apiScopes = apiScopes.Where(tbl => tbl.Paths == null).ToList();
                }
            }

            //if no scope is defined for the service, root access is required and request is not in scope
            if (apiScopes.Count == 0)
            {
                return new ApiScopeResult()
                {
                    AllowCreateAccount = false,
                    ScopeToUser = false,
                    IsInScope = false,
                    IsModerator = false
                };
            }

            ApiScopeResult result = new ApiScopeResult();

            result.IsInScope = true;
            //If any scope allows the create priveledge
            result.AllowCreateAccount = apiScopes.Any(tbl => tbl.Scope.Contains("create"));

            //Only scope to the account if the 'any' scope is present
            if (apiScopes.Any(tbl => tbl.Scope.Contains("any")))
                result.ScopeToUser = false;
            else
                result.ScopeToUser = true;

            result.IsModerator = roles.Contains("moderator");

            result.AllowRepeat = isFeed && (_contextAccessor?
                .HttpContext?
                .User?
                .Identity?
                .IsAuthenticated ?? false);

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
            if (!_contextAccessor.HttpContext.Request.Headers.TryGetValue("jma-source", out var source))
            {
                return null;
            }

            return source;
        }
    }
}
