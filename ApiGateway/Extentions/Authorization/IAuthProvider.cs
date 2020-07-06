using AspNetCore.ApiGateway;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.Models.Auth;

namespace ApiGateway.Extentions.Authorization
{
    public interface IAuthProvider
    {
        /// <summary>
        /// Checks agains the auth.json file if the current user may access a service
        /// </summary>
        /// <param name="info">The ApiInfo as used for service discovery</param>
        /// <param name="path">The first part of the path after the service name. ex /service/user/{path}</param>
        /// <returns></returns>
        ApiScopeResult IsApiInScope(RouteInfo info, string path = null);
        string GetUserId();
        string GetSource();
    }
}
