using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataAPI.Web.Setups
{
    public interface ISetup
    {
        void Run(IServiceCollection services, IConfiguration configuration);
    }
}
