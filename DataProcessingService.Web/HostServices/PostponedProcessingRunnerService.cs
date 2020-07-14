using System;
using System.Threading;
using System.Threading.Tasks;
using DataProcessing.ProcessingPostponing;
using Microsoft.Extensions.Hosting;

namespace DataProcessingService.Web.HostServices
{
    public class PostponedProcessingRunnerService : IHostedService
    {
        private readonly PostponedProcessingRunner postponedProcessingRunner;

        public PostponedProcessingRunnerService(PostponedProcessingRunner postponedProcessingRunner)
        {
            this.postponedProcessingRunner = postponedProcessingRunner;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            postponedProcessingRunner.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                postponedProcessingRunner.Stop();
            }
            catch
            {
                // Ignore
            }
            return Task.CompletedTask;
        }
    }
}
