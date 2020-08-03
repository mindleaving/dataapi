using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using Commons.Extensions;
using DataProcessing;

namespace DataProcessingService
{
    public partial class DataProcessingService : ServiceBase
    {
        [DllImport("advapi32.dll", SetLastError=true)]  
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);
        private ServiceStatus serviceStatus = new ServiceStatus
        {
            dwWaitHint = 100000
        };

        private readonly DataProcessingServiceSetup serviceSetup;

        public DataProcessingService(DataProcessingServiceSetup serviceSetup)
        {
            this.serviceSetup = serviceSetup;
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            SetServiceState(ServiceState.SERVICE_START_PENDING);
            serviceSetup.Distributor.Start();
            serviceSetup.PostponedProcessingRunner.Start();
            serviceSetup.PeriodicTasksRunner.Start();
            SetServiceState(ServiceState.SERVICE_RUNNING);
        }

        protected override void OnStop()
        {
            SetServiceState(ServiceState.SERVICE_STOP_PENDING);
            try
            {
                serviceSetup.Distributor.Stop();
            }
            catch
            {
                // Ignore
            }
            try
            {
                serviceSetup.PostponedProcessingRunner.Stop();
            }
            catch
            {
                // Ignore
            }
            try
            {
                serviceSetup.PeriodicTasksRunner.Stop();
            }
            catch
            {
                // Ignore
            }

            foreach (var disposable in serviceSetup.Processors.OfType<IDisposable>())
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
            foreach (var disposable in serviceSetup.Tasks.OfType<IDisposable>())
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
            SetServiceState(ServiceState.SERVICE_STOPPED);

        }

        private void SetServiceState(ServiceState state)
        {
            serviceStatus.dwCurrentState = state;
            SetServiceStatus(ServiceHandle, ref serviceStatus);
        }
    }
}
