using System.Windows;
using DataExplorerWpf.ViewModels;
using SharedViewModels.ViewModels;

namespace DataExplorerWpf.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IClosable
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(MainViewModel), typeof(MainWindow), new PropertyMetadata(default(MainViewModel)));

        public MainWindow()
        {
            InitializeComponent();
        }

        public MainViewModel ViewModel
        {
            get { return (MainViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if(!ViewModel.UserSwitchViewModel.IsLoggedIn)
                ViewModel?.UserSwitchViewModel.OpenLoginWindowCommand.Execute(this);
            else
                ViewModel?.LoginSuccessful();
        }
        
        public void Close(bool? dialogResult)
        {
            DialogResult = dialogResult;
            Close();
        }
    }
}
