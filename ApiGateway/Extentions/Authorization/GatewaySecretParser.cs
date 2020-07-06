using System;
using System.Text;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;

namespace ApiGateway.Extentions
{
    public class GatewaySecretParser : ISecretParser
    {
        /// <summary>
        /// 
        /// </summary>
        public string AuthenticationMethod => "MD5";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<ParsedSecret> ParseAsync(HttpContext context)
        {
            string authHeader = context.Request.Headers["Authorization"];

            if (authHeader != null && authHeader.StartsWith("Basic"))
            {
                authHeader = authHeader.Split(',')[0].Trim();

                string encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();
                string usernamePassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword));

                string[] parts = usernamePassword.Split(':');

                string username = "";
                string password = "";

                if (parts.Length > 1)
                    username = parts[0];

                if (parts.Length > 2)
                {
                    password = parts[1];
                }

                if (string.IsNullOrEmpty(username))
                    return null;

                if (string.IsNullOrEmpty(password))
                {
                    return new ParsedSecret()
                    {
                        Credential = "token",
                        Id = username,
                        Type = "token"
                    };
                }

                var parsedSecret = new ParsedSecret()
                {
                    Credential = password,
                    Id = username,
                    Type = "Basic"
                };

                return await Task.FromResult(parsedSecret);
            }

            return null;
        }
    }
}
