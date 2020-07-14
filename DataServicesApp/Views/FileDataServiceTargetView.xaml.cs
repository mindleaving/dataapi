using System.Windows;
using System.Windows.Controls;
using DataServicesApp.ViewModels;

namespace DataServicesApp.Views
{
    /// <summary>
    /// Interaction logic for FileDataServiceTargetView.xaml
    /// </summary>
    public partial class FileDataServiceTargetView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(FileDataServiceTargetViewModel), typeof(FileDataServiceTargetView), new PropertyMetadata(default(FileDataServiceTargetViewModel)));

        public FileDataServiceTargetView()
        {
            InitializeComponent();
        }

        public FileDataServiceTargetViewModel ViewModel
        {
            get { return (FileDataServiceTargetViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
