using DataAPI.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataAPI.Web.Setups
{
    public class LogSetup : ISetup
    {
        public void Run(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IEventLogger, ApiEventLogger>();
        }
    }
}
