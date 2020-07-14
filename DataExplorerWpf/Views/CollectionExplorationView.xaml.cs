using System.Windows;
using System.Windows.Controls;
using DataExplorerWpf.ViewModels;

namespace DataExplorerWpf.Views
{
    /// <summary>
    /// Interaction logic for CollectionExplorationView.xaml
    /// </summary>
    public partial class CollectionExplorationView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(CollectionExplorationViewModel), typeof(CollectionExplorationView), new PropertyMetadata(default(CollectionExplorationViewModel)));

        public CollectionExplorationView()
        {
            InitializeComponent();
        }

        public CollectionExplorationViewModel ViewModel
        {
            get { return (CollectionExplorationViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
