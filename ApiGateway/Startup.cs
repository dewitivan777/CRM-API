using ApiGateway.API;
using ApiGateway.Extentions;
using ApiGateway.Extentions.Authorization;
using ApiGateway.Extentions.Authorization.Services;
using AspNetCore.ApiGateway;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace ApiGateway
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IPasswordHasher<string>, GatewayPasswordHasher<string>>();
            services.AddSingleton<IAuthProvider, AuthProvider>();
            services.AddScoped<IIdCryptoProvider, IdCryptoProvider>();


            services.AddIdentityServer(options =>
                {
                    options.Events.RaiseSuccessEvents = true;
                })
                .AddDeveloperSigningCredential(persistKey: false)
                .AddInMemoryApiResources(Config.GetApiResources())
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddGatewayClientStore(Config.GetClients())
                .AddSecretValidator<GatewaySecretValidator>()
                .AddSecretParser<GatewaySecretParser>()
                .AddResourceOwnerValidator<GatewayResourceOwnerPasswordValidator>()
                .AddProfileService<GatewayProfileService>();

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "Automatic";
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options => Configuration.Bind("JwtSettings", options))
                .AddGatewayAuthentication("Automatic", "default gateway authentication scheme", options =>
                {
                    options.AuthenticationType = "Basic";
                });

            //Api gateway
            services.AddApiGateway();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My Api Gateway", Version = "v1" });
            });

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My Api Gateway");
            });

            app.UseMiddleware<RequestResponseLoggingMiddleware>();

            app.UseRouting();

            //Api gateway
            app.UseApiGateway(orchestrator => ApiOrchestration.Create(orchestrator, app));

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
