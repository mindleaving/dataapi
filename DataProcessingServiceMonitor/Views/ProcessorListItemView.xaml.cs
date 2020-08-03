using System.Windows;
using System.Windows.Controls;
using DataProcessingServiceMonitor.ViewModels;

namespace DataProcessingServiceMonitor.Views
{
    /// <summary>
    /// Interaction logic for ProcessorView.xaml
    /// </summary>
    public partial class ProcessorListItemView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(ProcessorViewModel), typeof(ProcessorListItemView), new PropertyMetadata(default(ProcessorViewModel)));

        public ProcessorListItemView()
        {
            InitializeComponent();
        }

        public ProcessorViewModel ViewModel
        {
            get { return (ProcessorViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
