using System.Windows;
using SharedViewModels.ViewModels;

namespace SharedWpfControls.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window, IClosable
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(LoginViewModel), typeof(LoginWindow), new PropertyMetadata(default(LoginViewModel)));

        public LoginWindow()
        {
            InitializeComponent();
        }

        public LoginViewModel ViewModel
        {
            get { return (LoginViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public void Close(bool? dialogResult)
        {
            DialogResult = dialogResult;
            Close();
        }
    }
}
