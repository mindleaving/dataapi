using System.Windows;
using DataProcessingServiceMonitor.ViewModels;

namespace DataProcessingServiceMonitor.Views
{
    /// <summary>
    /// Interaction logic for ProcessorTaskWindow.xaml
    /// </summary>
    public partial class ProcessorTaskWindow : Window
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(IExecutorViewModel), typeof(ProcessorTaskWindow), new PropertyMetadata(default(IExecutorViewModel)));

        public ProcessorTaskWindow()
        {
            InitializeComponent();
        }

        public IExecutorViewModel ViewModel
        {
            get { return (IExecutorViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
