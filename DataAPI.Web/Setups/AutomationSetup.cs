using DataAPI.Service.DataStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataAPI.Web.Setups
{
    public class AutomationSetup : ISetup
    {
        public void Run(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<NewCollectionTasks>();
        }
    }
}
