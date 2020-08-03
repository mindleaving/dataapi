using System.Windows;
using System.Windows.Controls;
using SharedViewModels.ViewModels;

namespace DataExplorerWpf.Views
{
    /// <summary>
    /// Interaction logic for DataObjectView.xaml
    /// </summary>
    public partial class DataObjectView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(DataObjectViewModel), typeof(DataObjectView), new PropertyMetadata(default(DataObjectViewModel)));

        public DataObjectView()
        {
            InitializeComponent();
        }

        public DataObjectViewModel ViewModel
        {
            get { return (DataObjectViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
