using System.Windows.Input;
using DataAPI.Client;
using DataAPI.Client.Repositories;
using DataServicesApp.Helpers;
using DataServicesApp.Models;
using DataServicesApp.Workflow;
using SharedViewModels.ViewModels;
using SharedWpfControls.Helpers;

namespace DataServicesApp.ViewModels
{
    public class MainViewModel : NotifyPropertyChangedBase
    {
        private readonly IClosable mainWindow;

        public MainViewModel(
            IDataApiClient dataApiClient,
            IObjectDatabase<DataServiceDefinition> dataServiceDefinitionDatabase,
            IClosable mainWindow,
            IDataTypeList dataTypeList,
            IUsernameProxy usernameProxy,
            ISqlExpressionValidator sqlExpressionValidator)
        {
            this.mainWindow = mainWindow;
            UserSwitchViewModel = new UserSwitchViewModelFactory(
                dataApiClient,
                LoginSuccessful,
                LoginFailed).Create();
            SqlExportViewModel = new SqlExportViewModel(dataServiceDefinitionDatabase, dataTypeList, usernameProxy, sqlExpressionValidator);
            ExitCommand = new RelayCommand<IClosable>(closable => closable.Close(true));
        }

        private void LoginSuccessful()
        {
            // Nothing to do
        }

        private void LoginFailed()
        {
            mainWindow.Close(false);
        }

        public SqlExportViewModel SqlExportViewModel { get; }
        public ICommand ExitCommand { get; }
        public UserSwitchViewModel UserSwitchViewModel { get; }
    }
}
