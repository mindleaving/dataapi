using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using Commons.Misc;
using DataAPI.Client;
using DataAPI.DataStructures.UserManagement;
using DataProcessing;

namespace DataProcessingService
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main()
        {
            RedirectAssembly();

            var apiConfiguration = new ApiConfiguration(
                ConfigurationManager.AppSettings["ApiServerAddress"],
                ushort.Parse(ConfigurationManager.AppSettings["ApiServerPort"]));
            var dataApiClient = new DataApiClient(apiConfiguration);
            var apiLoginInformation = new LoginInformation(
                ConfigurationManager.AppSettings["ApiUsername"],
                Secrets.Get(ConfigurationManager.AppSettings["DataApiPasswordEnvironmentVariableName"]));
            var serviceSetup = new DataProcessingServiceSetup(
                dataApiClient,
                apiLoginInformation,
                new DataProcessingServiceSettings
                {
                    ProcessorDefinitionDirectory = ConfigurationManager.AppSettings["ProcessorDefinitionDirectory"],
                    TaskDefinitionDirectory = ConfigurationManager.AppSettings["TaskDefinitionDirectory"]
                });
            var servicesToRun = new ServiceBase[]
            {
                new DataProcessingService(serviceSetup)
            };
            ServiceBase.Run(servicesToRun);
        }

        private static void RedirectAssembly()
        {
            var list = AppDomain.CurrentDomain.GetAssemblies().OrderByDescending(a => a.FullName).Select(a => a.FullName).ToList();
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var requestedAssembly = new AssemblyName(args.Name);
            Assembly assembly = null;
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
            try
            {
                assembly = Assembly.Load(requestedAssembly.Name);
            }
            catch
            {
                // Ignore
            }
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            return assembly;
        }
    }

    
}
