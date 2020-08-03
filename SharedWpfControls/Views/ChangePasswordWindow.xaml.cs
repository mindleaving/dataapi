using System.Windows;
using SharedViewModels.ViewModels;

namespace SharedWpfControls.Views
{
    /// <summary>
    /// Interaction logic for ChangePasswordWindow.xaml
    /// </summary>
    public partial class ChangePasswordWindow : Window, IClosable
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(ChangePasswordViewModel), typeof(ChangePasswordWindow), new PropertyMetadata(default(ChangePasswordViewModel)));

        public ChangePasswordWindow()
        {
            InitializeComponent();
        }

        public ChangePasswordViewModel ViewModel
        {
            get { return (ChangePasswordViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public void Close(bool? dialogResult)
        {
            DialogResult = dialogResult;
            Close();
        }
    }
}
