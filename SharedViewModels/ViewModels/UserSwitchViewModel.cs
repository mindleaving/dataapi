using System;
using System.Windows.Input;
using DataAPI.Client;
using SharedViewModels.Helpers;

namespace SharedViewModels.ViewModels
{
    public class UserSwitchViewModel : NotifyPropertyChangedBase
    {
        private readonly IDataApiClient dataApiClient;
        private readonly Action loginSuccessfulCallback;
        private readonly Action loginFailedCallback;
        private readonly IViewSpawner<LoginViewModel> loginViewSpawner;

        public UserSwitchViewModel(
            IDataApiClient dataApiClient,
            Action loginSuccessfulCallback,
            Action loginFailedCallback,
            IPasswordBoxSpawner passwordBoxSpawner,
            IViewSpawner<LoginViewModel> loginViewSpawner,
            IViewSpawner<ChangePasswordViewModel> changePasswordViewSpawner)
        {
            this.dataApiClient = dataApiClient;
            this.loginSuccessfulCallback = loginSuccessfulCallback;
            this.loginFailedCallback = loginFailedCallback;
            this.loginViewSpawner = loginViewSpawner;
            LoginViewModel = new LoginViewModel(dataApiClient, passwordBoxSpawner, changePasswordViewSpawner);
            OpenLoginWindowCommand = new RelayCommand(OpenLoginWindow);
        }

        public LoginViewModel LoginViewModel {get;}
        public ICommand OpenLoginWindowCommand { get; }
        public bool IsLoggedIn => dataApiClient.IsLoggedIn;
        public string LoggedInUser => dataApiClient.LoggedInUsername;

        private void OpenLoginWindow()
        {
            var originalUsername = LoginViewModel.Username;
            var originalPassword = LoginViewModel.Password;
            do
            {
                var result = loginViewSpawner.SpawnBlocking(LoginViewModel);
                if (result == true)
                {
                    var authenticationResult = dataApiClient.Login(LoginViewModel.Username, LoginViewModel.Password);
                    if(authenticationResult.IsAuthenticated)
                    {
                        loginSuccessfulCallback();
                        break;
                    }
                    StaticMessageBoxSpawner.Show("Login failed. Try again...");
                }
                else if(originalUsername != null)
                {
                    LoginViewModel.Username = originalUsername;
                    LoginViewModel.Password = originalPassword;
                    break;
                }
                else
                {
                    break;
                }
            } while (true);
            if (!dataApiClient.IsLoggedIn)
                loginFailedCallback();
        }

        public void TriggerOnPropertyChangedEvents()
        {
            OnPropertyChanged(nameof(IsLoggedIn));
            OnPropertyChanged(nameof(LoggedInUser));
        }
    }
}