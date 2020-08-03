using System.Windows;
using System.Windows.Controls;
using DataServicesApp.ViewModels;

namespace DataServicesApp.Views
{
    /// <summary>
    /// Interaction logic for CreateDataServiceTargetView.xaml
    /// </summary>
    public partial class EditDataServiceTargetView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(EditDataServiceTargetViewModel), typeof(EditDataServiceTargetView), new PropertyMetadata(default(EditDataServiceTargetViewModel)));

        public EditDataServiceTargetView()
        {
            InitializeComponent();
        }

        public EditDataServiceTargetViewModel ViewModel
        {
            get { return (EditDataServiceTargetViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
