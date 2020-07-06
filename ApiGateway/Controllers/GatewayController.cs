using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using AspNetCore.ApiGateway;
using AspNetCore.ApiGateway.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ApiGateway.Controllers
{
    [Route("service")]
    [ApiController]
    public class GatewayController  : ControllerBase
    {
        readonly IApiOrchestrator _apiOrchestrator;
        readonly ILogger<ApiGatewayLog> _logger;
        readonly IHttpService _httpService;


        public GatewayController(IApiOrchestrator apiOrchestrator, ILogger<ApiGatewayLog> logger, IHttpService httpService)
        {
            _apiOrchestrator = apiOrchestrator;
            _logger = logger;
            _httpService = httpService;
        }

        [HttpGet("{serviceName}/{*page}")] 
        [ServiceFilter(typeof(GatewayGetOrHeadAuthorizeAttribute))]
        public async Task<IActionResult> Get(string serviceName, string page)
        {
            var parameters = Request.QueryString.Value;

            if (parameters != null)
                parameters = HttpUtility.UrlDecode(parameters);
            else
                parameters = string.Empty;

            _logger.LogApiInfo(serviceName, page, parameters);

            var apiInfo = _apiOrchestrator.GetApi(serviceName);

            var gwRouteInfo = apiInfo.Mediator.GetRoute(page.ToLower()+GatewayVerb.GET);

            var routeInfo = gwRouteInfo.Route;

            if (routeInfo.Exec != null)
            {
                return Ok(await routeInfo.Exec(apiInfo, this.Request));
            }
            else
            {
                using (var client = routeInfo.HttpClientConfig?.HttpClient())
                {
                    this.Request.Headers?.AddRequestHeaders((client ?? _httpService.Client).DefaultRequestHeaders);

                    if (client == null)
                    {
                        routeInfo.HttpClientConfig?.CustomizeDefaultHttpClient?.Invoke(_httpService.Client, this.Request);
                    }

                    _logger.LogApiInfo($"{apiInfo.BaseUrl}{routeInfo.Path}{parameters}");

                    var response = await (client ?? _httpService.Client).GetAsync($"{apiInfo.BaseUrl}{routeInfo.Path}{parameters}");

                    response.EnsureSuccessStatusCode();

                    _logger.LogApiInfo($"{apiInfo.BaseUrl}{routeInfo.Path}{parameters}", false);

                    return Ok(routeInfo.ResponseType != null
                        ? JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync(), routeInfo.ResponseType)
                        : await response.Content.ReadAsStringAsync());
                }
            }
        }

        [HttpPost]
        [Route("{serviceName}/{*page}")]
        [ServiceFilter(typeof(GatewayPostAuthorizeAttribute))]
        public async Task<IActionResult> Post(string serviceName, string page, object request, string parameters = null)
        {
            if (parameters != null)
                parameters = HttpUtility.UrlDecode(parameters);
            else
                parameters = string.Empty;

            _logger.LogApiInfo(serviceName, page, parameters, request);

            var apiInfo = _apiOrchestrator.GetApi(serviceName);

            var gwRouteInfo = apiInfo.Mediator.GetRoute(page.ToLower() + GatewayVerb.POST);

            var routeInfo = gwRouteInfo.Route;

            if (routeInfo.Exec != null)
            {
                return Ok(await routeInfo.Exec(apiInfo, this.Request));
            }
            else
            {
                using (var client = routeInfo.HttpClientConfig?.HttpClient())
                {
                    HttpContent content = null;

                    if (routeInfo.HttpClientConfig?.HttpContent != null)
                    {
                        content = routeInfo.HttpClientConfig.HttpContent();
                    }
                    else
                    {
                        content = new StringContent(request.ToString(), Encoding.UTF8, "application/json");

                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    }

                    this.Request.Headers?.AddRequestHeaders((client ?? _httpService.Client).DefaultRequestHeaders);

                    if (client == null)
                    {
                        routeInfo.HttpClientConfig?.CustomizeDefaultHttpClient?.Invoke(_httpService.Client, this.Request);
                    }

                    _logger.LogApiInfo($"{apiInfo.BaseUrl}{routeInfo.Path}{parameters}");

                    var response = await (client ?? _httpService.Client).PostAsync($"{apiInfo.BaseUrl}{routeInfo.Path}{parameters}", content);

                    _logger.LogApiInfo($"{apiInfo.BaseUrl}{routeInfo.Path}{parameters}", false);

                    response.EnsureSuccessStatusCode();

                    return Ok(routeInfo.ResponseType != null
                        ? JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync(), routeInfo.ResponseType)
                        : await response.Content.ReadAsStringAsync());
                }
            }
        }

        [HttpPut]
        [Route("{serviceName}/{*page}")]
        [ServiceFilter(typeof(GatewayPutAuthorizeAttribute))]
        public async Task<IActionResult> Put(string serviceName, string page, object request, string parameters = null)
        {
            if (parameters != null)
                parameters = HttpUtility.UrlDecode(parameters);
            else
                parameters = string.Empty;

            _logger.LogApiInfo(serviceName, page, parameters, request);

            var apiInfo = _apiOrchestrator.GetApi(serviceName);

            var gwRouteInfo = apiInfo.Mediator.GetRoute(page.ToLower() + GatewayVerb.PUT);

            var routeInfo = gwRouteInfo.Route;

            if (routeInfo.Exec != null)
            {
                return Ok(await routeInfo.Exec(apiInfo, this.Request));
            }
            else
            {
                using (var client = routeInfo.HttpClientConfig?.HttpClient())
                {
                    HttpContent content = null;

                    if (routeInfo.HttpClientConfig?.HttpContent != null)
                    {
                        content = routeInfo.HttpClientConfig.HttpContent();
                    }
                    else
                    {
                        content = new StringContent(request.ToString(), Encoding.UTF8, "application/json");

                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    }

                    this.Request.Headers?.AddRequestHeaders((client ?? _httpService.Client).DefaultRequestHeaders);

                    if (client == null)
                    {
                        routeInfo.HttpClientConfig?.CustomizeDefaultHttpClient?.Invoke(_httpService.Client, this.Request);
                    }

                    _logger.LogApiInfo($"{apiInfo.BaseUrl}{routeInfo.Path}{parameters}");

                    var response = await (client ?? _httpService.Client).PutAsync($"{apiInfo.BaseUrl}{routeInfo.Path}{parameters}", content);

                    _logger.LogApiInfo($"{apiInfo.BaseUrl}{routeInfo.Path}{parameters}", false);

                    response.EnsureSuccessStatusCode();

                    return Ok(routeInfo.ResponseType != null
                        ? JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync(), routeInfo.ResponseType)
                        : await response.Content.ReadAsStringAsync());
                }
            }
        }

        [HttpPatch]
        [Route("{serviceName}/{*page}")]
        [ServiceFilter(typeof(GatewayPatchAuthorizeAttribute))]
        public async Task<IActionResult> Patch(string serviceName, string page, [FromBody] JsonPatchDocument<object> patch, string parameters = null)
        {
            if (parameters != null)
                parameters = HttpUtility.UrlDecode(parameters);
            else
                parameters = string.Empty;

            _logger.LogApiInfo(serviceName, page, parameters, patch.ToString());

            var apiInfo = _apiOrchestrator.GetApi(serviceName);

            var gwRouteInfo = apiInfo.Mediator.GetRoute(page.ToLower() + GatewayVerb.PATCH);

            var routeInfo = gwRouteInfo.Route;

            if (routeInfo.Exec != null)
            {
                return Ok(await routeInfo.Exec(apiInfo, this.Request));
            }
            else
            {
                using (var client = routeInfo.HttpClientConfig?.HttpClient())
                {
                    HttpContent content = null;

                    if (routeInfo.HttpClientConfig?.HttpContent != null)
                    {
                        content = routeInfo.HttpClientConfig.HttpContent();
                    }
                    else
                    {
                        var p = JsonConvert.SerializeObject(patch);

                        content = new StringContent(p, Encoding.UTF8, "application/json-patch+json");
                    }

                    this.Request.Headers?.AddRequestHeaders((client ?? _httpService.Client).DefaultRequestHeaders);

                    if (client == null)
                    {
                        routeInfo.HttpClientConfig?.CustomizeDefaultHttpClient?.Invoke(_httpService.Client, this.Request);
                    }

                    _logger.LogApiInfo($"{apiInfo.BaseUrl}{routeInfo.Path}{parameters}");

                    var response = await (client ?? _httpService.Client).PatchAsync($"{apiInfo.BaseUrl}{routeInfo.Path}{parameters}", content);

                    _logger.LogApiInfo($"{apiInfo.BaseUrl}{routeInfo.Path}{parameters}", false);

                    response.EnsureSuccessStatusCode();

                    return Ok(routeInfo.ResponseType != null
                        ? JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync(), routeInfo.ResponseType)
                        : await response.Content.ReadAsStringAsync());
                }
            }
        }

        [HttpDelete]
        [Route("{serviceName}/{*page}")]
        [ServiceFilter(typeof(GatewayDeleteAuthorizeAttribute))]
        public async Task<IActionResult> Delete(string serviceName, string page, string parameters = null)
        {
            if (parameters != null)
            {
                parameters = HttpUtility.UrlDecode(parameters);
            }
            else
                parameters = string.Empty;

            _logger.LogApiInfo(serviceName, page, parameters);

            var apiInfo = _apiOrchestrator.GetApi(serviceName);

            var gwRouteInfo = apiInfo.Mediator.GetRoute(page.ToLower() + GatewayVerb.DELETE);

            var routeInfo = gwRouteInfo.Route;

            if (routeInfo.Exec != null)
            {
                return Ok(await routeInfo.Exec(apiInfo, this.Request));
            }
            else
            {
                using (var client = routeInfo.HttpClientConfig?.HttpClient())
                {
                    this.Request.Headers?.AddRequestHeaders((client ?? _httpService.Client).DefaultRequestHeaders);

                    if (client == null)
                    {
                        routeInfo.HttpClientConfig?.CustomizeDefaultHttpClient?.Invoke(_httpService.Client, this.Request);
                    }

                    _logger.LogApiInfo($"{apiInfo.BaseUrl}{routeInfo.Path}{parameters}");

                    var response = await (client ?? _httpService.Client).DeleteAsync($"{apiInfo.BaseUrl}{routeInfo.Path}{parameters}");

                    _logger.LogApiInfo($"{apiInfo.BaseUrl}{routeInfo.Path}{parameters}", false);

                    response.EnsureSuccessStatusCode();

                    return Ok(routeInfo.ResponseType != null
                        ? JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync(), routeInfo.ResponseType)
                        : await response.Content.ReadAsStringAsync());
                }
            }
        }

        [HttpGet]
        [Route("orchestration")]
        [ServiceFilter(typeof(GatewayGetOrchestrationAuthorizeAttribute))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Orchestration))]
        public async Task<IActionResult> GetOrchestration(string serviceName = null, string page = null)
        {
            serviceName = serviceName?.ToLower();
            page = page?.ToLower();

            return Ok(await Task.FromResult(string.IsNullOrEmpty(serviceName) && string.IsNullOrEmpty(page)
                                            ? _apiOrchestrator.Orchestration
                                            : (!string.IsNullOrEmpty(serviceName) && string.IsNullOrEmpty(page)
                                            ? _apiOrchestrator.Orchestration?.Where(x => x.Api.Contains(serviceName.Trim()))
                                            : (string.IsNullOrEmpty(serviceName) && !string.IsNullOrEmpty(page)
                                            ? _apiOrchestrator.Orchestration?.Where(x => x.Routes.Any(y => y.Key.Contains(page.Trim())))
                                                                             .Select(x => x.FilterRoutes(page))
                                            : _apiOrchestrator.Orchestration?.Where(x => x.Api.Contains(serviceName.Trim()))
                                                                             .Select(x => x.FilterRoutes(page))))));
        }
    }
}