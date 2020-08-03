using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Commons.Extensions;
using DataAPI.Client;
using SharedViewModels.Helpers;
using SharedViewModels.Objects;

namespace SharedViewModels.ViewModels
{
    public class ChangePasswordViewModel : NotifyPropertyChangedBase
    {
        private readonly IDataApiClient dataApiClient;
        

        public ChangePasswordViewModel(
            IDataApiClient dataApiClient, 
             
            IPasswordBoxSpawner passwordBoxSpawner,
            string username = null)
        {
            this.dataApiClient = dataApiClient;
            
            Username = username;
            OldPasswordBox = passwordBoxSpawner.SpawnNew();
            NewPasswordBox = passwordBoxSpawner.SpawnNew(tabIndex: 2);
            NewPasswordRepeatBox = passwordBoxSpawner.SpawnNew(tabIndex: 3);
            ChangePasswordCommand = new AsyncRelayCommand<IClosable>(ChangeAndClose, CanSubmit);
            CancelCommand = new RelayCommand<IClosable>(closable => closable.Close(false));
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

        public IPasswordBox OldPasswordBox { get; }
        public IPasswordBox NewPasswordBox { get; }
        public IPasswordBox NewPasswordRepeatBox { get; }

        private bool isWorking;
        public bool IsWorking
        {
            get => isWorking;
            private set
            {
                isWorking = value;
                OnPropertyChanged();
            }
        }

        public IAsyncCommand ChangePasswordCommand { get; }
        public ICommand CancelCommand { get; }

        private bool CanSubmit(IClosable closable)
        {
            return !string.IsNullOrWhiteSpace(Username)
                   && !string.IsNullOrWhiteSpace(OldPasswordBox.Password)
                   && !string.IsNullOrWhiteSpace(NewPasswordBox.Password)
                   && NewPasswordBox.Password == NewPasswordRepeatBox.Password;
        }

        private async Task ChangeAndClose(IClosable closable)
        {
            if (NewPasswordBox.Password.Length < 6)
            {
                StaticMessageBoxSpawner.Show("New password is too short");
                return;
            }

            if (NewPasswordBox.Password == OldPasswordBox.Password)
            {
                StaticMessageBoxSpawner.Show("New and old password are identical");
                return;
            }

            IsWorking = true;
            var authenticationResult = await Task.Run(() => dataApiClient.Login(Username, OldPasswordBox.Password));
            if (!authenticationResult.IsAuthenticated)
            {
                StaticMessageBoxSpawner.Show("Username or old password are incorrect");
                IsWorking = false;
                return;
            }

            try
            {
                await Task.Run(() => dataApiClient.ChangePassword(Username, NewPasswordBox.Password));
                dataApiClient.Logout();
                StaticMessageBoxSpawner.Show($"Successfully changed password for '{Username}'");
                closable.Close(true);
            }
            catch (Exception e)
            {
                StaticMessageBoxSpawner.Show($"Could not change password: {e.InnermostException().Message}");
            }
            finally
            {
                IsWorking = false;
            }
        }
    }
}
