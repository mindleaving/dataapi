using System.Windows;
using System.Windows.Controls;
using DataExplorerWpf.ViewModels;

namespace DataExplorerWpf.Views
{
    /// <summary>
    /// Interaction logic for DatasetMainView.xaml
    /// </summary>
    public partial class DatasetMainView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(DataSetMainViewModel), typeof(DatasetMainView), new PropertyMetadata(default(DataSetMainViewModel)));

        public DatasetMainView()
        {
            InitializeComponent();
        }

        public DataSetMainViewModel ViewModel
        {
            get { return (DataSetMainViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
