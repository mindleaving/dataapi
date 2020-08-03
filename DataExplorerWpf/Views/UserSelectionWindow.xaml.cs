using System.Windows;
using DataExplorerWpf.ViewModels;
using SharedViewModels.ViewModels;

namespace DataExplorerWpf.Views
{
    /// <summary>
    /// Interaction logic for UserPermissionEditWindow.xaml
    /// </summary>
    public partial class UserSelectionWindow : Window, IClosable
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(UserSelectionViewModel), typeof(UserSelectionWindow), new PropertyMetadata(default(UserSelectionViewModel)));

        public UserSelectionWindow()
        {
            InitializeComponent();
        }

        public UserSelectionViewModel ViewModel
        {
            get { return (UserSelectionViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public void Close(bool? dialogResult)
        {
            DialogResult = dialogResult;
            Close();
        }
    }
}
