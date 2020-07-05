using Client.Models;
using Microsoft.AspNetCore.Http;
using ServiceRegistry;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    public abstract class ApiClientBase
    {
        private readonly HttpClient _httpClient;
        private readonly IServiceRegistry _serviceRegistry;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly List<string> _allowedHeaders = new List<string>()
        {
            "jma-page",
            "jma-ip",
            "jma-gaid",
            "jma-devicetype",
            "jma-source",
            "jma-reason",
            "jma-notify",
            "jma-message"
        };

        public ApiClientBase(
            HttpClient httpClient,
            IServiceRegistry serviceRegistry,
            IHttpContextAccessor contextAccessor)
        {
            _httpClient = httpClient;
            _serviceRegistry = serviceRegistry;
            _contextAccessor = contextAccessor;
        }

        protected async Task<IApiResponse<T>> SendAsync<T>(
            ApiInfo serviceInfo,
            HttpRequestMessage httpRequestMessage,
            string token,
            CancellationToken cancellationToken = default,
            AuditInfo audit = null,
            string userClaim = null) where T : class
        {
            HttpResponseMessage httpResponseMessage;
            try
            {
                var serviceLocation = await GetServiceLocation(serviceInfo);

                if (string.IsNullOrWhiteSpace(serviceLocation))
                {
                    return new ApiResponse<T>(HttpStatusCode.NotFound, $"{serviceInfo?.Name} {serviceInfo?.Method} service not found");
                }

                httpRequestMessage.RequestUri = new Uri(serviceLocation + httpRequestMessage.RequestUri.ToString());

                if (_contextAccessor.HttpContext != null)
                {
                    foreach (var header in _contextAccessor.HttpContext.Request.Headers)
                    {
                        if (header.Key.StartsWith("jma-"))
                        {
                            if (_allowedHeaders.Contains(header.Key))
                                httpRequestMessage.Headers.Add(header.Key, header.Value.ToArray());
                        }
                    }

                    if (!_contextAccessor.HttpContext.Request.Headers.Keys.Contains("jma-ip"))
                    {
                        httpRequestMessage.Headers.Add("jma-ip", _contextAccessor.HttpContext.GetRequestIP());
                    }

                    if (_contextAccessor.HttpContext.User != null)
                    {
                        List<string> claims = new List<string>();

                        foreach (Claim claim in _contextAccessor.HttpContext.User.Claims)
                        {
                            if (claim.Type == ClaimTypes.NameIdentifier)
                            {
                                httpRequestMessage.Headers.Add("jma-user-id", claim.Value);
                            }
                            if (claim.Type == "source")
                            {
                                httpRequestMessage.Headers.Add("jma-reportingsource", claim.Value);
                            }
                            if (claim.Type == "group")
                            {
                                httpRequestMessage.Headers.Add("jma-group", claim.Value);
                            }
                            else if (claim.Type == "preferred_username")
                            {
                                httpRequestMessage.Headers.Add("jma-user-name", claim.Value);
                            }
                            //TODO: this role check may have caused a world of issues
                            else if (claim.Type == ClaimTypes.Role)
                            {
                                claims.Add(claim.Type + ":" + claim.Value);
                            }
                        }

                        httpRequestMessage.Headers.Add("jma-claims", string.Join(",", claims.ToArray()));
                    }


                    if (userClaim != null)
                    {
                        httpRequestMessage.Headers.Add("jma-user-id", userClaim);
                    }

                    httpRequestMessage.Headers.Add("jma-page", _contextAccessor.HttpContext.Request.ToUrlString());
                }

                if (audit != null)
                {
                    if (!string.IsNullOrEmpty(audit.Reason))
                    {
                        httpRequestMessage.Headers.Add("jma-reason", audit.Reason);
                    }

                    if (!string.IsNullOrEmpty(audit.Message))
                    {
                        httpRequestMessage.Headers.Add("jma-message", audit.Message);
                    }

                    if (audit.DontSendNotification)
                    {
                        httpRequestMessage.Headers.Add("jma-notify", "false");
                    }
                }

                if (!string.IsNullOrEmpty(token))
                {
                    httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage, cancellationToken);
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>(ex);
            }

            string raw = null;
            if (httpResponseMessage.Content != null)
            {
                raw = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            return new ApiResponse<T>(httpResponseMessage.StatusCode, raw, httpResponseMessage.ReasonPhrase);
        }

        private async Task<string> GetServiceLocation(ApiInfo serviceInfo)
        {
            var serviceLocation = await _serviceRegistry.GetServiceLocation(serviceInfo.Name, serviceInfo.Method);
            return serviceLocation;
        }
    }
}