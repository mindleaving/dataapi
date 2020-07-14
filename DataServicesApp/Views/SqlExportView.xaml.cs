using System.Windows;
using System.Windows.Controls;
using DataServicesApp.ViewModels;

namespace DataServicesApp.Views
{
    /// <summary>
    /// Interaction logic for SqlExportView.xaml
    /// </summary>
    public partial class SqlExportView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(SqlExportViewModel), typeof(SqlExportView), new PropertyMetadata(default(SqlExportViewModel)));

        public SqlExportView()
        {
            InitializeComponent();
        }

        public SqlExportViewModel ViewModel
        {
            get { return (SqlExportViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
