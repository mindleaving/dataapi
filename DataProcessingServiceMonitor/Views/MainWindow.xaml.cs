using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using DataProcessingServiceMonitor.ViewModels;
using SharedViewModels.ViewModels;

namespace DataProcessingServiceMonitor.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IClosable
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(MainViewModel), typeof(MainWindow), new PropertyMetadata(default(MainViewModel)));

        private readonly DispatcherTimer clockTimer;

        public MainWindow()
        {
            InitializeComponent();
            clockTimer = new DispatcherTimer(
                TimeSpan.FromSeconds(1), 
                DispatcherPriority.Normal,
                delegate { clockLabel.Content = DateTime.UtcNow.ToString("HH:mm:ss"); },
                Dispatcher);
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
            clockTimer.Start();
        }

        public void Close(bool? dialogResult)
        {
            DialogResult = dialogResult;
            Close();
        }

        private void OpenProcessorWindow(object sender, MouseButtonEventArgs e)
        {
            ViewModel?.OpenProcessorWindowCommand.Execute(null);
        }

        private void OpenTaskWindow(object sender, MouseButtonEventArgs e)
        {
            ViewModel?.OpenTaskWindowCommand.Execute(null);
        }
    }
}
