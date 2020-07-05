using Microsoft.Extensions.DependencyInjection;

namespace Client
{
    public static class ApiClientServiceCollectionExtentions
    {
        public static IServiceCollection AddApiClient(this IServiceCollection services)
        {
            services.AddSingleton<IHttpClientFactory, HttpClientFactory>();

            services.AddSingleton(provider =>
            {
                var factory = provider.GetService<IHttpClientFactory>();
                return factory.CreateHttpClient();
            });

            services.AddSingleton<IApiClient, ApiClient>();
            services.AddSingleton<INameResolver, NameResolver>();
            return services;
        }
    }
}