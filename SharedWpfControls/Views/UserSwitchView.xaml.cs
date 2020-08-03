using System.Windows;
using System.Windows.Controls;
using SharedViewModels.ViewModels;

namespace SharedWpfControls.Views
{
    /// <summary>
    /// Interaction logic for UserSwitchView.xaml
    /// </summary>
    public partial class UserSwitchView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(UserSwitchViewModel), typeof(UserSwitchView), new PropertyMetadata(default(UserSwitchViewModel)));

        public UserSwitchView()
        {
            InitializeComponent();
        }

        public UserSwitchViewModel ViewModel
        {
            get { return (UserSwitchViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
