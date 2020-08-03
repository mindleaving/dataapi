using System.Windows;
using DataServicesApp.ViewModels;
using SharedViewModels.ViewModels;

namespace DataServicesApp.Views
{
    /// <summary>
    /// Interaction logic for CreateDataServiceTargetWindow.xaml
    /// </summary>
    public partial class DataServiceTargetWindow : Window, IClosable
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(DataServiceTargetWindowViewModel), typeof(DataServiceTargetWindow), new PropertyMetadata(default(DataServiceTargetWindowViewModel)));

        public DataServiceTargetWindow()
        {
            InitializeComponent();
        }

        public DataServiceTargetWindowViewModel ViewModel
        {
            get { return (DataServiceTargetWindowViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public void Close(bool? dialogResult)
        {
            DialogResult = dialogResult;
            Close();
        }
    }
}
