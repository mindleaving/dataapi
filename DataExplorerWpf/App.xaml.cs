using System.Windows;
using DataAPI.Client;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures.DataManagement;
using DataExplorerWpf.Properties;
using DataExplorerWpf.ViewModels;
using DataExplorerWpf.Views;
using DataExplorerWpf.Visualization;
using SharedViewModels.Helpers;
using SharedViewModels.Repositories;
using SharedWpfControls.Helpers;

namespace DataExplorerWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            StaticMessageBoxSpawner.Spawner = new WpfMessageBoxSpawner();
            StaticUiUpdateNotifier.Notifier = new WpfUiUpdateNotifier();

            var apiConfiguration = new ApiConfiguration(Settings.Default.ApiServerAddress, Settings.Default.ApiServerPort);
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
            
            var dataVisualizer = new DataVisualizer(new IDataVisualizationViewModelFactory[]
            {
                new ImageVisualizationViewModelFactory(dataApiClient),
                // TODO: Add more entries here to custom visualization for data types
            });
            var userDatabase = new UserDatabase(dataApiClient);
            var dataSetDatabase = new GenericDatabase<DataSet>(dataApiClient);
            var dataProjectDatabase = new GenericDatabase<DataProject>(dataApiClient);
            var clipboard = new WpfClipboard();
            var mainWindow = new MainWindow();
            var mainViewModel = new MainViewModel(
                dataApiClient,
                dataVisualizer,
                userDatabase,
                dataSetDatabase,
                dataProjectDatabase,
                clipboard,
                mainWindow);
            mainWindow.ViewModel = mainViewModel;
            mainWindow.ShowDialog();
        }
    }
}
