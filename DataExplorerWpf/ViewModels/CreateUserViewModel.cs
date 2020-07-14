using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Commons.Extensions;
using DataAPI.Client;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures.UserManagement;
using SharedViewModels.Helpers;
using SharedViewModels.ViewModels;

namespace DataExplorerWpf.ViewModels
{
    public class CreateUserViewModel : NotifyPropertyChangedBase
    {
        private readonly IDataApiClient dataApiClient;
        private readonly IReadonlyObjectDatabase<UserProfile> userDatabase;

        public CreateUserViewModel(
            IReadonlyObjectDatabase<UserProfile> userDatabase, 
            IDataApiClient dataApiClient)
        {
            this.userDatabase = userDatabase;
            this.dataApiClient = dataApiClient;
            PasswordBox = new PasswordBox();
            PasswordRepeatBox = new PasswordBox();
            SaveCommand = new RelayCommand(SaveAndClose, CanSave);
        }

        private string username;
        public string Username
        {
            get => username;
            set
            {
                username = value;
                var exists = Task.Run(() => userDatabase.ExistsAsync(username).Result).Result;
                if(exists)
                    throw new Exception($"User '{username}' already exists");
                OnPropertyChanged();
            }
        }

        private string firstName;
        public string FirstName
        {
            get => firstName;
            set
            {
                firstName = value;
                OnPropertyChanged();
            }
        }

        private string lastName;
        public string LastName
        {
            get => lastName;
            set
            {
                lastName = value;
                OnPropertyChanged();
            }
        }

        private string email;
        public string Email
        {
            get => email;
            set
            {
                email = value;
                OnPropertyChanged();
            }
        }

        public PasswordBox PasswordBox { get; }
        public PasswordBox PasswordRepeatBox { get; }

        public bool IsPasswordRepeatCorrect => !string.IsNullOrEmpty(PasswordBox.Password)
                                               && PasswordBox.Password == PasswordRepeatBox.Password;

        public ICommand SaveCommand { get; }

        private bool CanSave()
        {
            return !string.IsNullOrEmpty(Username)
                && IsPasswordRepeatCorrect;
        }

        private void SaveAndClose()
        {
            try
            {
                dataApiClient.Register(Username, FirstName, LastName, PasswordBox.Password, Email);
                StaticMessageBoxSpawner.Show($"User '{Username}' successfully created");
                UserCreated?.Invoke(this, new UserProfileViewModel(new UserProfile(Username, FirstName, LastName, Email), new List<Role>()));
            }
            catch (Exception e)
            {
                StaticMessageBoxSpawner.Show("Could not create user: " + e.InnermostException().Message);
            }
        }

        public event EventHandler<UserProfileViewModel> UserCreated;
    }
}
