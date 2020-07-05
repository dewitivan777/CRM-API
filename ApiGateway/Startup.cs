using System.Text;
using ApiGateway.Auth;
using ApiGateway.Extentions;
using ApiGateway.Services;
using AuthService.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
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
            var jwtSection = Configuration.GetSection("jwt");
            var jwtOptions = jwtSection.Get<JwtOptions>();
            var key = Encoding.UTF8.GetBytes(jwtOptions.Secret);

            services.AddIdentityServer(options =>
                {
                    options.Events.RaiseSuccessEvents = true;
                })
                .AddDeveloperSigningCredential(persistKey: false)
                .AddInMemoryApiResources(Config.GetApiResources())
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddGatewayClientStore(Config.GetClients())
                //.AddCustomAuthorizeRequestValidator<GatewayAuthorizeRequestValidator>()
                //.AddCustomTokenRequestValidator<GatewayTokenRequestValidator>()
                .AddSecretValidator<GatewaySecretValidator>()
                .AddSecretParser<GatewaySecretParser>()
                .AddResourceOwnerValidator<GatewayResourceOwnerPasswordValidator>()
                .AddProfileService<GatewayProfileService>();

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "Automatic";
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options => Configuration.Bind("Jwt", options))
                .AddGatewayAuthentication("Automatic", "default gateway authentication scheme", options =>
                {
                    options.AuthenticationType = "Basic";
                });


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

            app.UseRouting();

            ////CORS
            app.UseCors("AllowAll");

            //IdentityServer4, for use with bearer authentication
            app.UseIdentityServer();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
