using System.Windows;
using System.Windows.Controls;
using DataExplorerWpf.ViewModels;

namespace DataExplorerWpf.Views
{
    /// <summary>
    /// Interaction logic for CollectionListView.xaml
    /// </summary>
    public partial class CollectionListView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(CollectionListViewModel), typeof(CollectionListView), new PropertyMetadata(default(CollectionListViewModel)));

        public CollectionListView()
        {
            InitializeComponent();
        }

        public CollectionListViewModel ViewModel
        {
            get { return (CollectionListViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
