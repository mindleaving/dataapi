using System;
using DataAPI.Client;
using SharedViewModels.ViewModels;
using SharedWpfControls.Views;

namespace SharedWpfControls.Helpers
{
    public class UserSwitchViewModelFactory
    {
        private readonly IDataApiClient dataApiClient;
        private readonly Action loginSuccessful;
        private readonly Action loginFailed;

        public UserSwitchViewModelFactory(IDataApiClient dataApiClient, Action loginSuccessful, Action loginFailed)
        {
            this.dataApiClient = dataApiClient;
            this.loginSuccessful = loginSuccessful;
            this.loginFailed = loginFailed;
        }

        public UserSwitchViewModel Create()
        {
            var passwordBoxSpawner = new WpfPasswordBoxSpawner();
            var loginViewSpawner = new WpfWindowSpawner<LoginViewModel>(vm => new LoginWindow { ViewModel = vm });
            var changePasswordViewSpawner = new WpfWindowSpawner<ChangePasswordViewModel>(vm => new ChangePasswordWindow { ViewModel = vm });
            return new UserSwitchViewModel(
                dataApiClient,
                loginSuccessful,
                loginFailed,
                passwordBoxSpawner,
                loginViewSpawner,
                changePasswordViewSpawner);
        }
    }
}
