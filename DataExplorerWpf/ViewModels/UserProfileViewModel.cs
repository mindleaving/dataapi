using System.Collections.Generic;
using System.Collections.ObjectModel;
using DataAPI.DataStructures.UserManagement;
using SharedViewModels.ViewModels;

namespace DataExplorerWpf.ViewModels
{
    public class UserProfileViewModel : NotifyPropertyChangedBase
    {
        public UserProfileViewModel(UserProfile userProfile, IList<Role> globalRoles)
        {
            Username = userProfile.Username;
            Firstname = userProfile.FirstName;
            Lastname = userProfile.LastName;
            Email = userProfile.Email;
            Roles = new ObservableCollection<Role>(globalRoles);
        }

        public string Username { get; }

        private string firstname;
        public string Firstname
        {
            get => firstname;
            set
            {
                firstname = value;
                OnPropertyChanged();
            }
        }

        private string lastname;
        public string Lastname
        {
            get => lastname;
            set
            {
                lastname = value;
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

        public ObservableCollection<Role> Roles { get; }
    }
}
