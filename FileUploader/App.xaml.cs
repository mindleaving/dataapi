using System.Windows;
using DataAPI.Client;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures;
using DataAPI.DataStructures.DataIo;
using DataAPI.DataStructures.DataManagement;
using FileHandlers;
using FileUploader.Properties;
using FileUploader.ViewModel;
using FileUploader.Views;
using SharedViewModels.Helpers;
using SharedWpfControls.Helpers;
using SharedWpfControls.Views;

namespace FileUploader
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
            var clipboard = new WpfClipboard();

            var startWindow = new StartupWindow {ApplicationName = "File uploader"};
            startWindow.Show();
            var apiConfiguration = new ApiConfiguration(
                Settings.Default.ApiServerAddress,
                Settings.Default.ApiServerPort);
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
            
            var imageDatabase = new GenericDatabase<Image>(dataApiClient);
            var shortIdDatabase = new GenericDatabase<ShortId>(dataApiClient);
            var dataSetDatabase = new GenericDatabase<DataSet>(dataApiClient);
            var tagDatabase = new GenericDatabase<DataTag>(dataApiClient);
            var projectDatabase = new GenericDatabase<DataProject>(dataApiClient);
            var protocolDatabase = new GenericDatabase<DataCollectionProtocol>(dataApiClient);
            var protocolParameterDatabase = new GenericDatabase<DataCollectionProtocolParameter>(dataApiClient);
            var dataBlobDatabase = new GenericDatabase<DataBlob>(dataApiClient);
            var dataProjectUploadInfoDatabase = new GenericDatabase<DataProjectUploadInfo>(dataApiClient);

            var fileHandlers = new IFileHandler[]
            {
                new ImageFileHandler(imageDatabase)
            };
            var additionalInformationViewSpawner = new WpfAdditionalInformationViewSpawner();
            var fileManager = new UploadedFileProcessor(
                fileHandlers,
                dataApiClient,
                dataBlobDatabase,
                dataProjectUploadInfoDatabase,
                clipboard,
                additionalInformationViewSpawner);

            var mainWindow = new MainWindow();
            var mainViewModel = new MainViewModel(
                mainWindow,
                dataApiClient,
                fileManager,
                dataSetDatabase,
                tagDatabase,
                projectDatabase,
                protocolDatabase,
                protocolParameterDatabase);
            mainWindow.ViewModel = mainViewModel;
            startWindow.Close();
            mainWindow.ShowDialog();
        }
    }
}
