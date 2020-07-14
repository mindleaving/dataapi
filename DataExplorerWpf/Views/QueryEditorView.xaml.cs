using System.Windows;
using System.Windows.Controls;
using DataExplorerWpf.ViewModels;

namespace DataExplorerWpf.Views
{
    /// <summary>
    /// Interaction logic for QueryEditorView.xaml
    /// </summary>
    public partial class QueryEditorView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(QueryEditorViewModel), typeof(QueryEditorView), new PropertyMetadata(default(QueryEditorViewModel)));

        public QueryEditorView()
        {
            InitializeComponent();
        }

        public QueryEditorViewModel ViewModel
        {
            get { return (QueryEditorViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
