using System;
using System.Windows;
using FileUploader.ViewModel;
using SharedViewModels.ViewModels;

namespace FileUploader.Views
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

        private async void DropBorderDrop(object sender, DragEventArgs e)
        {
            if(ViewModel == null)
                throw new NullReferenceException("View model is not initialized");
            if(!(sender is DependencyObject dragTarget))
                return;
            var hasData = e.Data.GetDataPresent(DataFormats.FileDrop);
            if (!hasData)
                return;
            var filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);
            if(filePaths == null)
                return;
            await ViewModel.HandleFileDrop(filePaths);
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
