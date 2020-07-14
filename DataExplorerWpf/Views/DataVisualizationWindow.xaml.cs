using System.Windows;
using DataExplorerWpf.ViewModels;
using SharedViewModels.ViewModels;

namespace DataExplorerWpf.Views
{
    /// <summary>
    /// Interaction logic for DataVisualizationWindow.xaml
    /// </summary>
    public partial class DataVisualizationWindow : Window, IClosable
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(IDataVisualizationViewModel), typeof(DataVisualizationWindow), new PropertyMetadata(default(IDataVisualizationViewModel)));

        public DataVisualizationWindow()
        {
            InitializeComponent();
        }

        public IDataVisualizationViewModel ViewModel
        {
            get { return (IDataVisualizationViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public void Close(bool? dialogResult)
        {
            DialogResult = dialogResult;
            Close();
        }
    }
}
