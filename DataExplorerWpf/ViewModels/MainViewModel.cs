using System.Windows.Input;
using DataAPI.Client;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures.DataManagement;
using DataAPI.DataStructures.UserManagement;
using DataExplorerWpf.Views;
using SharedViewModels.Helpers;
using SharedViewModels.Objects;
using SharedViewModels.ViewModels;
using SharedWpfControls.Helpers;
using SharedWpfControls.Views;

namespace DataExplorerWpf.ViewModels
{
    public class MainViewModel : NotifyPropertyChangedBase
    {
        private readonly IDataApiClient dataApiClient;
        private readonly IReadonlyObjectDatabase<UserProfile> userDatabase;
        private readonly IClosable mainWindow;

        public MainViewModel(
            IDataApiClient dataApiClient,
            DataVisualizer dataVisualizer,
            IReadonlyObjectDatabase<UserProfile> userDatabase,
            IReadonlyObjectDatabase<DataSet> dataSetDatabase,
            IReadonlyObjectDatabase<DataProject> dataProjectDatabase,
            IClipboard clipboard,
            IClosable mainWindow)
        {
            this.dataApiClient = dataApiClient;
            this.userDatabase = userDatabase;
            this.mainWindow = mainWindow;
            DataSetMainViewModel = new DataSetMainViewModel(dataApiClient, dataSetDatabase, dataProjectDatabase, clipboard);
            CollectionsMainViewModel = new CollectionsMainViewModel(dataApiClient, dataVisualizer, userDatabase, clipboard);

            var passwordBoxSpawner = new WpfPasswordBoxSpawner();
            var loginViewSpawner = new WpfWindowSpawner<LoginViewModel>(vm => new LoginWindow { ViewModel = vm });
            var changePasswordViewSpawner = new WpfWindowSpawner<ChangePasswordViewModel>(vm => new ChangePasswordWindow { ViewModel = vm });
            UserSwitchViewModel = new UserSwitchViewModel(
                dataApiClient,
                LoginSuccessful,
                LoginFailed,
                passwordBoxSpawner,
                loginViewSpawner,
                changePasswordViewSpawner);
            OpenUserManagementWindowCommand = new RelayCommand(OpenUserManagementWindow);
            OpenAboutWindowCommand = new RelayCommand(OpenAboutWindow);
        }

        public UserSwitchViewModel UserSwitchViewModel { get; }
        public DataSetMainViewModel DataSetMainViewModel { get; }
        public CollectionsMainViewModel CollectionsMainViewModel { get; }

        public ICommand OpenUserManagementWindowCommand { get; }
        public ICommand OpenAboutWindowCommand { get; }

        public async void LoginSuccessful()
        {
            await CollectionsMainViewModel.Reload();
            UserSwitchViewModel.TriggerOnPropertyChangedEvents();
        }

        private void LoginFailed()
        {
            StaticMessageBoxSpawner.Show("No valid login. Shutting down...");
            mainWindow.Close(false);
        }

        private void OpenUserManagementWindow()
        {
            userDatabase.DeleteCache();
            var userManagementViewModel = new UserManagementViewModel(dataApiClient, userDatabase);
            var userManagementWindow = new UserManagementWindow
            {
                ViewModel = userManagementViewModel
            };
            userManagementWindow.ShowDialog();
        }

        private void OpenAboutWindow()
        {
            new AboutWindow().ShowDialog();
        }
    }
}
