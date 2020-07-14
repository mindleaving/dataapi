using System;
using System.Threading;
using System.Threading.Tasks;
using DataProcessing;
using Microsoft.Extensions.Hosting;

namespace DataProcessingService.Web.HostServices
{
    public class DistributorService : IHostedService
    {
        private readonly Distributor distributor;

        public DistributorService(Distributor distributor)
        {
            this.distributor = distributor;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            distributor.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                distributor.Stop();
            }
            catch
            {
                // Ignore
            }
            return Task.CompletedTask;
        }
    }
}
