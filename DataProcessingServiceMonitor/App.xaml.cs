using System;
using System.Windows;
using DataAPI.Client;
using DataProcessingServiceMonitor.Properties;
using DataProcessingServiceMonitor.ViewModels;
using DataProcessingServiceMonitor.Views;
using SharedViewModels.Helpers;
using SharedWpfControls.Helpers;

namespace DataProcessingServiceMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            StaticMessageBoxSpawner.Spawner = new WpfMessageBoxSpawner();
            StaticUiUpdateNotifier.Notifier = new WpfUiUpdateNotifier();

            var apiConfiguration = new ApiConfiguration(Settings.Default.DataApiServerAddress, Settings.Default.DataApiServerPort);
            var dataApiClient = new DataApiClient(apiConfiguration);
            if (!dataApiClient.IsAvailable())
            {
                MessageBox.Show($"Cannot contact DataAPI @ '{apiConfiguration.ServerAddress}:{apiConfiguration.ServerPort}'. Shutting down...");
                Current.Shutdown(-1);
                return;
            }
            try
            {
                dataApiClient.Login();
            }
            catch
            {
                // Ignore. If Active Directory is not available, in some other way later.
            }

            var logEntryMonitor = new LogEntryMonitor(dataApiClient);

            var mainWindow = new MainWindow();
            var mainViewModel = new MainViewModel(dataApiClient, logEntryMonitor, mainWindow);
            mainWindow.ViewModel = mainViewModel;
            try
            {
                mainWindow.ShowDialog();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
                throw;
            }

            // Shutdown
            logEntryMonitor.StopMonitoring();
            Shutdown(0);
        }
    }
}
