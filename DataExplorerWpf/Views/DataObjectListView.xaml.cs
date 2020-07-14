using System.Windows;
using System.Windows.Controls;
using DataExplorerWpf.ViewModels;

namespace DataExplorerWpf.Views
{
    /// <summary>
    /// Interaction logic for DataObjectListView.xaml
    /// </summary>
    public partial class DataObjectListView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(DataObjectListViewModel), typeof(DataObjectListView), new PropertyMetadata(default(DataObjectListViewModel)));

        public DataObjectListView()
        {
            InitializeComponent();
        }

        public DataObjectListViewModel ViewModel
        {
            get { return (DataObjectListViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
