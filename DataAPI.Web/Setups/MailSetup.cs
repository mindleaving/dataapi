using Commons.Misc;
using DataAPI.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataAPI.Web.Setups
{
    public class MailSetup : ISetup
    {
        public void Run(IServiceCollection services, IConfiguration configuration)
        {
            var mailServerAddress = configuration["Mail:ServerAddress"];
            var mailServerPort = ushort.Parse(configuration["Mail:ServerPort"]);
            var mailFromAddress = configuration["Mail:From"];
            var mailUseSSL = bool.Parse(configuration["Mail:UseSSL"]);
            var mailAuthenticationRequired = bool.Parse(configuration["Mail:UseSSL"]);
            var mailPasswordEnvironmentVariableName = configuration["Mail:PasswordEnvironmentVariable"];

            if (string.IsNullOrEmpty(mailServerAddress))
            {
                services.AddSingleton<IMailSender, NoMailSender>();
            }
            else
            {
                // Test mail password availability. Will throw exception if it doesn't exist
                Secrets.Get(mailPasswordEnvironmentVariableName);
                services.AddSingleton<IMailSender>(
                    provider =>
                    {
                        var apiEventLogger = provider.GetService<IEventLogger>();
                        return new MailSender(
                            mailServerAddress,
                            mailServerPort,
                            mailFromAddress,
                            mailUseSSL,
                            mailAuthenticationRequired,
                            mailPasswordEnvironmentVariableName,
                            apiEventLogger);
                    });
            }
        }
    }
}
