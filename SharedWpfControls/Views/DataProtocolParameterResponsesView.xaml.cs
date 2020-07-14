using System.Windows;
using System.Windows.Controls;
using SharedViewModels.ViewModels;

namespace SharedWpfControls.Views
{
    /// <summary>
    /// Interaction logic for ParameterResponsesView.xaml
    /// </summary>
    public partial class DataProtocolParameterResponsesView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(ProjectParameterResponsesViewModel), typeof(DataProtocolParameterResponsesView), new PropertyMetadata(default(ProjectParameterResponsesViewModel)));

        public DataProtocolParameterResponsesView()
        {
            InitializeComponent();
        }

        public ProjectParameterResponsesViewModel ViewModel
        {
            get { return (ProjectParameterResponsesViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
