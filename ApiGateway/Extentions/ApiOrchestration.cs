using AspNetCore.ApiGateway;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Services.Domain.Classification.Models;

namespace ApiGateway.API
{
    public static class ApiOrchestration
    {
        public static void Create(IApiOrchestrator orchestrator, IApplicationBuilder app)
        {
            var serviceProvider = app.ApplicationServices;

            // var weatherService = serviceProvider.GetService<IWeatherService>();

            // var weatherApiClientConfig = weatherService.GetClientConfig();

            orchestrator.AddApi("ClassificationService", "http://localhost:5002/")
                //Get
                .AddRoute("category", GatewayVerb.GET,
                    new RouteInfo
                    {
                        Path = "ClassificationService/category", ResponseType = typeof(IEnumerable<WeatherForecast>),
                        Scope = new ApiScope() {Scope = "Any", SubClaims = new List<string>() {"Any"}}
                    })
                .AddRoute("category", GatewayVerb.POST,
                    new RouteInfo
                    {
                        Path = "ClassificationService/category",
                        ResponseType = typeof(IEnumerable<WeatherForecast>),
                        Scope = new ApiScope() {Scope = "Any", SubClaims = new List<string>() {"Moderator"}}
                    });




            //        //Head
            //        .AddRoute("forecasthead", GatewayVerb.HEAD, new RouteInfo { Path = "weatherforecast/forecast" })
            //        //Get using custom HttpClient
            //        .AddRoute("types", GatewayVerb.GET, new RouteInfo { Path = "weatherforecast/types", ResponseType = typeof(string[]), HttpClientConfig = weatherApiClientConfig })
            //        //Get with param using custom HttpClient
            //        .AddRoute("type", GatewayVerb.GET, new RouteInfo { Path = "weatherforecast/types/", ResponseType = typeof(WeatherTypeResponse), HttpClientConfig = weatherApiClientConfig })
            //        //Get using custom implementation
            //        .AddRoute("typescustom", GatewayVerb.GET, weatherService.GetTypes)
            //        //Post
            //        .AddRoute("add", GatewayVerb.POST, new RouteInfo { Path = "weatherforecast/types/add", RequestType = typeof(AddWeatherTypeRequest), ResponseType = typeof(string[])})
            //        //Put
            //        .AddRoute("update", GatewayVerb.PUT, new RouteInfo { Path = "weatherforecast/types/update", RequestType = typeof(UpdateWeatherTypeRequest), ResponseType = typeof(string[]) })
            //        //Patch
            //        .AddRoute("patch", GatewayVerb.PATCH, new RouteInfo { Path = "weatherforecast/forecast/patch", ResponseType = typeof(WeatherForecast) })
            //        //Delete
            //        .AddRoute("remove", GatewayVerb.DELETE, new RouteInfo { Path = "weatherforecast/types/remove/", ResponseType = typeof(string[]) })
            //.AddApi("stockservice", "http://localhost:63967/")
            //        .AddRoute("stocks", GatewayVerb.GET, new RouteInfo { Path = "stock", ResponseType = typeof(IEnumerable<StockQuote>) })
            //        .AddRoute("stock", GatewayVerb.GET, new RouteInfo { Path = "stock/", ResponseType = typeof(StockQuote) });
        }
    }
}
