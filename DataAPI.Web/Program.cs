using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

#pragma warning disable 1591

namespace DataAPI.Web
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseKestrel(
                        options =>
                        {
                            options.Limits.MaxRequestBodySize = long.MaxValue;
                        });
                });
    }
}
