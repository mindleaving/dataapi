using System;
using System.Threading;
using System.Threading.Tasks;
using DataProcessing;
using Microsoft.Extensions.Hosting;

namespace DataProcessingService.Web.HostServices
{
    public class PeriodicTasksRunnerService : IHostedService
    {
        private readonly PeriodicTasksRunner periodicTasksRunner;

        public PeriodicTasksRunnerService(PeriodicTasksRunner periodicTasksRunner)
        {
            this.periodicTasksRunner = periodicTasksRunner;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            periodicTasksRunner.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                periodicTasksRunner.Stop();
            }
            catch
            {
                // Ignore
            }
            return Task.CompletedTask;
        }
    }
}
