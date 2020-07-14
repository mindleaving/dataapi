using System.Windows;
using System.Windows.Controls;
using DataExplorerWpf.ViewModels;

namespace DataExplorerWpf.Views
{
    /// <summary>
    /// Interaction logic for CollectionsMainView.xaml
    /// </summary>
    public partial class CollectionsMainView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(CollectionsMainViewModel), typeof(CollectionsMainView), new PropertyMetadata(default(CollectionsMainViewModel)));

        public CollectionsMainView()
        {
            InitializeComponent();
        }

        public CollectionsMainViewModel ViewModel
        {
            get { return (CollectionsMainViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
