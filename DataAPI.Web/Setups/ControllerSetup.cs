using DataAPI.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataAPI.Web.Setups
{ 
    public class ControllerSetup : ISetup
    {
        public void Run(IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers(
                    options =>
                    {
                        options.Filters.Add(new ProducesAttribute(Conventions.JsonContentType));
                    })
                .AddNewtonsoftJson();
        }
    }
}
