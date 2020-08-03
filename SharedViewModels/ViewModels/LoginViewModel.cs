using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Commons.Extensions;
using DataAPI.Client;
using SharedViewModels.Helpers;
using SharedViewModels.Objects;

namespace SharedViewModels.ViewModels
{
    public class LoginViewModel : NotifyPropertyChangedBase
    {
        private readonly IDataApiClient dataApiClient;
        
        private readonly IPasswordBoxSpawner passwordBoxSpawner;
        private readonly IViewSpawner<ChangePasswordViewModel> changePasswordViewSpawner;

        public LoginViewModel(
            IDataApiClient dataApiClient, 
            IPasswordBoxSpawner passwordBoxSpawner,
            IViewSpawner<ChangePasswordViewModel> changePasswordViewSpawner)
        {
            this.dataApiClient = dataApiClient;
            this.passwordBoxSpawner = passwordBoxSpawner;
            this.changePasswordViewSpawner = changePasswordViewSpawner;

            Username = dataApiClient.LoggedInUsername;
            PasswordBox = passwordBoxSpawner.SpawnNew();
            ChangePasswordCommand = new RelayCommand<IClosable>(OpenChangePasswordWindow);
            LoginCommand = new RelayCommand<IClosable>(Login, CanLogin);
            CancelCommand = new RelayCommand<IClosable>(closable => closable?.Close(false), closable => !IsLoggingIn);
        }

        private string username;
        public string Username
        {
            get => username;
            set
            {
                username = value;
                OnPropertyChanged();
            }
        }

        public object PasswordBox { get; }
        public string Password
        {
            get => ((IPasswordBox) PasswordBox).Password;
            set => ((IPasswordBox) PasswordBox).Password = value;
        }

        private bool isLoggingIn;
        public bool IsLoggingIn
        {
            get => isLoggingIn;
            private set
            {
                isLoggingIn = value;
                OnPropertyChanged();
            }
        }

        public ICommand ChangePasswordCommand { get; }
        public ICommand LoginCommand { get; }
        public ICommand CancelCommand { get; }

        private void OpenChangePasswordWindow(IClosable closable)
        {
            var changePasswordViewModel = new ChangePasswordViewModel(dataApiClient, passwordBoxSpawner, Username);
            if(changePasswordViewSpawner.SpawnBlocking(changePasswordViewModel) != true)
                return;
            Username = changePasswordViewModel.Username;
            Password = changePasswordViewModel.NewPasswordBox.Password;
            LoginCommand.Execute(closable);
        }

        private bool CanLogin(IClosable closable)
        {
            return !IsLoggingIn
                   && !string.IsNullOrEmpty(Username)
                   && !string.IsNullOrEmpty(Password);
        }

        private async void Login(IClosable closable)
        {
            IsLoggingIn = true;
            try
            {
                var authenticationResult = await Task.Run(() => dataApiClient.Login(Username, Password));
                if (authenticationResult.IsAuthenticated)
                {
                    closable?.Close(true);
                }
                else
                {
                    StaticMessageBoxSpawner.Show("Could not authenticate. Check username and password");
                }
            }
            catch (Exception e)
            {
                StaticMessageBoxSpawner.Show($"Error: {e.InnermostException().Message}");
            }
            finally
            {
                IsLoggingIn = false;
            }
        }
    }
}
