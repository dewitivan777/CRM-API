using System;
using System.Threading.Tasks;
using Client.Models;
using System.Net.Http;
using System.Text;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using ServiceRegistry;

namespace Client
{
    public class ApiClient : ApiClientBase, IApiClient
    {
        public ApiClient(
            HttpClient httpClient,
            IServiceRegistry serviceRegistry,
            IHttpContextAccessor contextAccessor) : base(httpClient, serviceRegistry, contextAccessor)
        {
        }

        public async Task<IApiResponse<T>> CreateAsync<T, TContent>(
            ApiInfo serviceInfo,
            TContent content,
            string pathWithQuery,
            string token = null,
            AuditInfo audit = null)
            where T : class
            where TContent : class
        {
            if (content is string) return new ApiResponse<T>(new ArgumentException("Content should not be a string"));

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, pathWithQuery)
            {
                Content = new StringContent(
                    content: content.ToJsonString(),
                    encoding: Encoding.UTF8,
                    mediaType: "application/json")
            };

            return await SendAsync<T>(serviceInfo, httpRequestMessage, token, audit: audit);
        }

        public async Task<IApiResponse<T>> DeleteAsync<T>(
            ApiInfo serviceInfo,
            string pathWithQuery,
            string token = null,
            AuditInfo audit = null)
            where T : class
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, pathWithQuery);

            return await SendAsync<T>(serviceInfo, httpRequestMessage, token, audit: audit);
        }

        public async Task<IApiResponse<T>> EditAsync<T, TContent>(
            ApiInfo serviceInfo,
            TContent content,
            string pathWithQuery,
            string token = null,
            AuditInfo audit = null)
            where T : class
            where TContent : class
        {
            if (content is string) return new ApiResponse<T>(new ArgumentException("content should not be a string"));

            var stringContent = content.ToJsonString();

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, pathWithQuery)
            {
                Content = new StringContent(content: stringContent, encoding: Encoding.UTF8, mediaType: "application/json")
            };

            return await SendAsync<T>(serviceInfo, httpRequestMessage, token, audit: audit);
        }

        public async Task<IApiResponse<T>> GetAsync<T>(
            ApiInfo serviceInfo,
            string pathWithQuery,
            string token = null,
            string userClaim = null)
            where T : class
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, pathWithQuery);

            return await SendAsync<T>(serviceInfo, httpRequestMessage, token, userClaim: userClaim);
        }

        public async Task<IApiResponse<T>> GetByQueryAsync<T>(
            ApiInfo serviceInfo,
            string pathWithQuery,
            string token = null,
            params KeyValuePair<string, string>[] queryValues)
            where T : class
        {
            using (var content = new FormUrlEncodedContent(queryValues))
            {
                var query = await content.ReadAsStringAsync();

                var requestWithQuery = string.Concat(pathWithQuery, "?", query);

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, requestWithQuery);

                return await SendAsync<T>(serviceInfo, httpRequestMessage, token);
            }
        }
    }
}