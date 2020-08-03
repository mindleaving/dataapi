using System.Windows;
using System.Windows.Controls;
using DataExplorerWpf.ViewModels;

namespace DataExplorerWpf.Views
{
    /// <summary>
    /// Interaction logic for DataProjectView.xaml
    /// </summary>
    public partial class DataProjectView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(DataProjectViewModel), typeof(DataProjectView), new PropertyMetadata(default(DataProjectViewModel)));

        public DataProjectView()
        {
            InitializeComponent();
        }

        public DataProjectViewModel ViewModel
        {
            get { return (DataProjectViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
