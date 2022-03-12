using DataAPI.Service;
using DataAPI.Web.Setups;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

#pragma warning disable 1591

namespace DataAPI.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var setups = new ISetup[]
            {
                new CorsSetup(),
                new LogSetup(),
                new ControllerSetup(),
                new AccessControlSetup(),
                new StoreSetup(),
                new MailSetup(),
                new AutomationSetup(),
                new OpenApiSetup()
            };
            foreach (var setup in setups)
            {
                setup.Run(services, Configuration);
            }
        }

        // ReSharper disable once UnusedMember.Global
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseSwagger();
            app.UseSwaggerUI(
                c =>
                {
                    c.SwaggerEndpoint($"/swagger/v{ApiVersion.Current}/swagger.json", "DataAPI");
                });

            //app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
