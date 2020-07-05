using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ApiGateway.Auth;
using ApiGateway.Models;
using Client;
using Client.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ApiGateway.Controllers
{
    [Route("service")]
    [ApiController]
    public class GatewayController : ControllerBase
    {
        private readonly IApiClient _apiClient;

        private readonly IAuthProvider _authProvider;

        private readonly INameResolver _nameResolver;

        public GatewayController(
            IApiClient apiClient,
            IAuthProvider authProvider,
            INameResolver nameResolver)
        {
            _apiClient = apiClient;
            _authProvider = authProvider;
            _nameResolver = nameResolver;
        }

        [HttpGet("{serviceName}/{*page}")]
        public async Task<IActionResult> Get(string serviceName, string page)
        {
            var apiInfo = new ApiInfo(serviceName.ToLowerInvariant(), method: "query");

            ApiScopeResult scope = _authProvider.IsApiInScope(apiInfo, page);

            if (!scope.IsInScope)
            {
                //Return not found to avoid unwanted consumers discovering services
                return NotFound();
            }

            string pathWithQuery;

            if (scope.ScopeToUser)
            {
                if (Request.QueryString.HasValue)
                {
                    //Requests that should be in scope may not add the userid parameter
                    if (Request.QueryString.ToString().ToLower().Contains("userid"))
                        return NotFound();
                }

                string userId = _authProvider.GetUserId();

                if (string.IsNullOrEmpty(userId))
                    return NotFound();

                pathWithQuery = Request.QueryString.HasValue ?
                      Request.Path.Value + Request.QueryString + $"&userId={userId}"
                    : Request.Path.Value + $"?userId={userId}";
            }
            else
            {
                pathWithQuery = Request.QueryString.HasValue ? Request.Path.Value + Request.QueryString : Request.Path.Value;
            }

            var response = await _apiClient.GetAsync<object>(apiInfo, pathWithQuery);

            if (response.IsError)
            {
                if (response.ResponseError == ResponseError.Http)
                {
                    if (string.IsNullOrWhiteSpace(response.Raw))
                    {
                        return StatusCode((int)response.HttpStatusCode);
                    }
                    else
                    {
                        return StatusCode((int)response.HttpStatusCode, response.Raw);
                    }
                }

                if (!string.IsNullOrWhiteSpace(response.Error))
                {
                    return StatusCode(500, response.Error);
                }

                return StatusCode(500, "unexpected response received while executing the request");
            }

            return Ok(response.Content);
        }

        [HttpPost("{serviceName}/{*page}")]
        public async Task<IActionResult> Create([FromBody]object @object, string serviceName)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var apiInfo = new ApiInfo(serviceName.ToLowerInvariant(), method: "command");

            ApiScopeResult scope = _authProvider.IsApiInScope(apiInfo);

            if (!scope.IsInScope)
            {
                return NotFound();
            }

            if (scope.ScopeToUser)
            {
                PropertyInfo propertyInfo = @object.GetType().GetProperty("UserId");

                if (propertyInfo == null)
                    return BadRequest("The requested type requires a UserId");

                propertyInfo.SetValue(@object, _authProvider.GetUserId());
            }

            var pathWithQuery = Request.QueryString.HasValue ? Request.Path.Value + Request.QueryString : Request.Path.Value;

            var response = await _apiClient.CreateAsync<object, object>(apiInfo, @object, pathWithQuery);

            if (response.IsError)
            {
                if (response.ResponseError == ResponseError.Http)
                {
                    if (response.HttpStatusCode == HttpStatusCode.BadRequest)
                    {
                        ModelState.AddModelError(string.Empty, response.Raw);
                        return BadRequest(ModelState);
                    }

                    return StatusCode((int)response.HttpStatusCode, response.Error);
                }

                if (response.ResponseError == ResponseError.Exception)
                {
                    return StatusCode(500, response.Error);
                }
                return StatusCode(500);
            }

            return Ok(response.Content);
        }

        [HttpPut("{serviceName}/{*page}")]
        public async Task<IActionResult> Edit([FromBody]object @object, string serviceName)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var apiInfo = new ApiInfo(serviceName.ToLowerInvariant(), method: "command");

            ApiScopeResult scope = _authProvider.IsApiInScope(apiInfo);

            if (!scope.IsInScope)
            {
                return NotFound();
            }

            if (scope.ScopeToUser)
            {
                string id = Request.Path.Value.Split('/').Last();

                var checkResponse = await _apiClient.GetAsync<UserIdResponse>(
                        new ApiInfo(name: apiInfo.Name, method: "query"),
                        pathWithQuery: $"/service/{apiInfo.Name}?id={id}"
                    );

                if (checkResponse?.Content?.UserId != _authProvider.GetUserId())
                {
                    return BadRequest($"No {apiInfo.Name} found to edit");
                }

                PropertyInfo propertyInfo = @object.GetType().GetProperty("UserId");

                if (propertyInfo == null)
                    return BadRequest("The requested type requires a UserId");

                propertyInfo.SetValue(@object, _authProvider.GetUserId());
            }

            var pathWithQuery = Request.QueryString.HasValue ? Request.Path.Value + Request.QueryString : Request.Path.Value;

            @object = await _nameResolver.ResolveNamesAsync(@object);

            var response = await _apiClient.EditAsync<object, object>(apiInfo, @object, pathWithQuery);

            if (response.IsError)
            {
                if (response.ResponseError == ResponseError.Http)
                {
                    if (response.HttpStatusCode == HttpStatusCode.NotFound) return NotFound();

                    if (response.HttpStatusCode == HttpStatusCode.BadRequest)
                    {
                        ModelState.AddModelError(string.Empty, response.Raw);
                        return BadRequest(ModelState);
                    }

                    return StatusCode((int)response.HttpStatusCode, response.Error);
                }

                return StatusCode(500, response.Error);
            }

            return Ok(response.Content);
        }

        [HttpDelete("{serviceName}/{*page}")]
        public async Task<IActionResult> Delete(string serviceName)
        {
            var apiInfo = new ApiInfo(serviceName.ToLowerInvariant(), method: "command");

            var pathWithQuery = Request.QueryString.HasValue ? Request.Path.Value + Request.QueryString : Request.Path.Value;

            ApiScopeResult scope = _authProvider.IsApiInScope(apiInfo);

            if (!scope.IsInScope)
            {
                return NotFound();
            }

            if (scope.ScopeToUser)
            {
                string id = Request.Path.Value.Split('/').Last();

                var checkResponse = await _apiClient.GetAsync<UserIdResponse>(
                        new ApiInfo(name: apiInfo.Name, method: "query"),
                        pathWithQuery: $"/service/{apiInfo.Name}?id={id}"
                    );

                if (checkResponse?.Content?.UserId != _authProvider.GetUserId())
                {
                    return BadRequest($"No {apiInfo.Name} found to delete");
                }
            }

            var response = await _apiClient.DeleteAsync<object>(apiInfo, pathWithQuery);

            if (response.IsError)
            {
                if (response.ResponseError == ResponseError.Http)
                {
                    if (response.HttpStatusCode == HttpStatusCode.NotFound) return NotFound();

                    return StatusCode((int)response.HttpStatusCode);
                }

                if (response.ResponseError == ResponseError.Exception)
                {
                    return StatusCode(500, response.Error);
                }

                return StatusCode(500);
            }

            return Ok(response.Content);
        }
    }
}