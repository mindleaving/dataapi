using System.Windows;
using System.Windows.Controls;
using DataProcessingServiceMonitor.ViewModels;

namespace DataProcessingServiceMonitor.Views
{
    /// <summary>
    /// Interaction logic for ProcessorDetailsView.xaml
    /// </summary>
    public partial class ProcessorDetailsView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(ProcessorDetails), typeof(ProcessorDetailsView), new PropertyMetadata(default(ProcessorDetails)));

        public ProcessorDetailsView()
        {
            InitializeComponent();
        }

        public ProcessorDetails ViewModel
        {
            get { return (ProcessorDetails) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
