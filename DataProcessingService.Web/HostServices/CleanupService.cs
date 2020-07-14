using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataProcessing;
using Microsoft.Extensions.Hosting;

namespace DataProcessingService.Web.HostServices
{
    public class CleanupService : IHostedService
    {
        private readonly DataProcessingServiceSetup dataProcessingServiceSetup;

        public CleanupService(DataProcessingServiceSetup dataProcessingServiceSetup)
        {
            this.dataProcessingServiceSetup = dataProcessingServiceSetup;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var disposable in dataProcessingServiceSetup.Processors.OfType<IDisposable>())
            {
                try
                {
                    disposable.Dispose();
                }
                catch
                {
                    // Ignore
                }
            }
            foreach (var disposable in dataProcessingServiceSetup.Tasks.OfType<IDisposable>())
            {
                try
                {
                    disposable.Dispose();
                }
                catch
                {
                    // Ignore
                }
            }
            return Task.CompletedTask;
        }
    }
}
