using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ServiceRegistry;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RegistryServiceCollectionExtentions
    {
        public static IServiceCollection AddServiceRegistry(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RegistryOptions>(configuration);

            services.TryAddSingleton(provider =>
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                return client;
            });

            services.AddSingleton<IServiceRegistry, MemoryServiceRegistry>();

            return services;
        }
    }
}
