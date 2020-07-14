using System.Threading.Tasks;
using Commons.Misc;
using DataAPI.Client;
using DataAPI.DataStructures.UserManagement;
using DataProcessing;
using DataProcessingService.Web.Configuration;
using DataProcessingService.Web.HostServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace DataProcessingService.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddOptions();
                    services.Configure<DataApiSettings>(hostContext.Configuration.GetSection("DataAPI"));
                    services.Configure<DataProcessingServiceSettings>(hostContext.Configuration.GetSection("DataProcessingService"));
                    services.AddSingleton(provider =>
                    {
                        var settings = provider.GetService<IOptions<DataApiSettings>>().Value;
                        return new ApiConfiguration(settings.ServerAddress, settings.ServerPort);
                    });
                    services.AddSingleton<IDataApiClient, DataApiClient>();
                    services.AddSingleton(provider =>
                    {
                        var settings = provider.GetService<IOptions<DataApiSettings>>().Value;
                        return new LoginInformation(settings.Username, Secrets.Get(settings.PasswordEnvironmentVariableName));
                    });
                    services.AddSingleton(
                        provider =>
                        {
                            var dataApiClient = provider.GetService<IDataApiClient>();
                            var apiLoginInformation = provider.GetService<LoginInformation>();
                            var settings = provider.GetService<IOptions<DataProcessingServiceSettings>>().Value;
                            return new DataProcessingServiceSetup(dataApiClient, apiLoginInformation, settings);
                        });
                    services.AddSingleton(provider => provider.GetService<DataProcessingServiceSetup>().Distributor);
                    services.AddSingleton(provider => provider.GetService<DataProcessingServiceSetup>().PeriodicTasksRunner);
                    services.AddSingleton(provider => provider.GetService<DataProcessingServiceSetup>().PostponedProcessingRunner);

                    services.AddHostedService<DistributorService>();
                    services.AddHostedService<PeriodicTasksRunnerService>();
                    services.AddHostedService<PostponedProcessingRunnerService>();
                    services.AddHostedService<CleanupService>();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
