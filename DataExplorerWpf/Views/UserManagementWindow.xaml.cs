using System.Windows;
using DataExplorerWpf.ViewModels;
using SharedViewModels.ViewModels;

namespace DataExplorerWpf.Views
{
    /// <summary>
    /// Interaction logic for UserManagementWindow.xaml
    /// </summary>
    public partial class UserManagementWindow : Window, IClosable
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(UserManagementViewModel), typeof(UserManagementWindow), new PropertyMetadata(default(UserManagementViewModel)));

        public UserManagementWindow()
        {
            InitializeComponent();
        }

        public UserManagementViewModel ViewModel
        {
            get { return (UserManagementViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public void Close(bool? dialogResult)
        {
            DialogResult = dialogResult;
            Close();
        }
    }
}
