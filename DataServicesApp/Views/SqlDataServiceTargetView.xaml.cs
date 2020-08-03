using System.Windows;
using System.Windows.Controls;
using DataServicesApp.ViewModels;

namespace DataServicesApp.Views
{
    /// <summary>
    /// Interaction logic for SqlDataServiceTargetView.xaml
    /// </summary>
    public partial class SqlDataServiceTargetView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(SqlDataServiceTargetViewModel), typeof(SqlDataServiceTargetView), new PropertyMetadata(default(SqlDataServiceTargetViewModel)));

        public SqlDataServiceTargetView()
        {
            InitializeComponent();
        }

        public SqlDataServiceTargetViewModel ViewModel
        {
            get { return (SqlDataServiceTargetViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
