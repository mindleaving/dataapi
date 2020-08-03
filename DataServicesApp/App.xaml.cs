using System.Collections.Generic;
using System.Windows;
using DataAPI.Client;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures.Distribution;
using DataAPI.DataStructures.UserManagement;
using DataServicesApp.Helpers;
using DataServicesApp.Models;
using DataServicesApp.Properties;
using DataServicesApp.ViewModels;
using DataServicesApp.Views;
using DataServicesApp.Workflow;
using SharedViewModels.Helpers;
using SharedWpfControls.Helpers;

namespace DataServicesApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private async void App_OnStartup(object sender, StartupEventArgs e)
        {
            StaticUiUpdateNotifier.Notifier = new WpfUiUpdateNotifier();
            StaticMessageBoxSpawner.Spawner = new WpfMessageBoxSpawner();

            var dataApiClient = new DataApiClient(new ApiConfiguration(Settings.Default.ApiServerAddress, Settings.Default.ApiServerPort));
            dataApiClient.Login();
            if (!dataApiClient.IsLoggedIn)
            {
                StaticMessageBoxSpawner.Show("Could not log into DataAPI. Shutting down...");
                Shutdown(-1);
                return;
            }
            var permissionCheckerResult = await DataPermissionChecker.Check(dataApiClient, new Dictionary<string, IList<Role>>
            {
                {nameof(DataServiceDefinition), new List<Role> { Role.Viewer, Role.DataProducer} },
                {nameof(IDataServiceTarget), new List<Role> { Role.Viewer, Role.DataProducer} }
            });
            if (!permissionCheckerResult.HasSufficientPermissions)
            {
                StaticMessageBoxSpawner.Show("You don't have enough permissions on the DataAPI to run this application. Please contact Inno-IT.");
                Shutdown(-1);
                return;
            }
            var dataServiceDefinitionDatabase = new GenericDatabase<DataServiceDefinition>(dataApiClient);
            var dataTypeList = new DataApiDataTypeList(dataApiClient);
            var usernameProxy = new DataApiUsernameProxy(dataApiClient);
            var sqlExpressionValidator = new SqlExpressionValidator(dataApiClient);
            var mainWindow = new MainWindow();
            var mainViewModel = new MainViewModel(
                dataApiClient,
                dataServiceDefinitionDatabase,
                mainWindow,
                dataTypeList,
                usernameProxy,
                sqlExpressionValidator);
            mainWindow.ViewModel = mainViewModel;
            mainWindow.ShowDialog();
        }
    }
}
