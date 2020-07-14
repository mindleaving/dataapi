using System.Windows.Input;
using AutoCompleteMatchers;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures.UserManagement;
using SharedViewModels.ViewModels;

namespace DataExplorerWpf.ViewModels
{
    public class UserSelectionViewModel : NotifyPropertyChangedBase
    {
        public UserSelectionViewModel(IReadonlyObjectDatabase<UserProfile> userDatabase)
        {
            UserAutoCompleteViewModel = new AutoCompleteTextBoxViewModel<UserProfile>(
                x => $"{x.FirstName} {x.LastName} ({x.Username})",
                userDatabase,
                new UserAutoCompleteMatcher());
            OkCommand = new RelayCommand<IClosable>(SaveAndClose, CanSave);
            CancelCommand = new RelayCommand<IClosable>(closable => closable.Close(false));
        }

        public AutoCompleteTextBoxViewModel<UserProfile> UserAutoCompleteViewModel { get; }
        public UserProfile SelectedUser => UserAutoCompleteViewModel.SelectedObject;

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        private bool CanSave(IClosable arg)
        {
            return UserAutoCompleteViewModel.SelectedObject != null;
        }

        private void SaveAndClose(IClosable closable)
        {
            closable.Close(true);
        }
    }
}
