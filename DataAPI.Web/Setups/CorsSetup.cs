using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataAPI.Web.Setups
{
    public class CorsSetup : ISetup
    {
        private const string CorsPolicyName = "CorsPolicy";

        public void Run(IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicyName, builder =>
                {
                    var origins = configuration.GetSection("CORS:AllowedOrigins").Get<string[]>();
                    builder.WithOrigins(origins)
                        .AllowCredentials()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
        }
    }
}
