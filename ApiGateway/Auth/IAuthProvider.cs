using ApiGateway.Models;
using Client.Models;

namespace ApiGateway.Auth
{
    public interface IAuthProvider
    {
        /// <summary>
        /// Checks agains the auth.json file if the current user may access a service
        /// </summary>
        /// <param name="info">The ApiInfo as used for service discovery</param>
        /// <param name="path">The first part of the path after the service name. ex /service/user/{path}</param>
        /// <returns></returns>
        ApiScopeResult IsApiInScope(ApiInfo info, string path = null);
        string GetUserId();
        string GetSource();
    }
}
