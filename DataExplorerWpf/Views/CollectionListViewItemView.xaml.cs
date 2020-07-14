using System.Windows;
using System.Windows.Controls;
using DataExplorerWpf.ViewModels;

namespace DataExplorerWpf.Views
{
    /// <summary>
    /// Interaction logic for CollectionListViewItemView.xaml
    /// </summary>
    public partial class CollectionListViewItemView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(CollectionViewModel), typeof(CollectionListViewItemView), new PropertyMetadata(default(CollectionViewModel)));

        public CollectionListViewItemView()
        {
            InitializeComponent();
        }

        public CollectionViewModel ViewModel
        {
            get { return (CollectionViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
