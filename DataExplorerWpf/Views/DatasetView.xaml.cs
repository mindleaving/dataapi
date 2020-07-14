using System.Windows;
using System.Windows.Controls;
using DataExplorerWpf.ViewModels;

namespace DataExplorerWpf.Views
{
    /// <summary>
    /// Interaction logic for DatasetView.xaml
    /// </summary>
    public partial class DataSetView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(DataSetViewModel), typeof(DataSetView), new PropertyMetadata(default(DataSetViewModel)));

        public DataSetView()
        {
            InitializeComponent();
        }

        public DataSetViewModel ViewModel
        {
            get { return (DataSetViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
