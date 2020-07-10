using System.Windows;
using System.Windows.Controls;
using DataProcessingServiceMonitor.ViewModels;

namespace DataProcessingServiceMonitor.Views
{
    /// <summary>
    /// Interaction logic for TaskDetailsView.xaml
    /// </summary>
    public partial class TaskDetailsView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(TaskDetails), typeof(TaskDetailsView), new PropertyMetadata(default(TaskDetails)));

        public TaskDetailsView()
        {
            InitializeComponent();
        }

        public TaskDetails ViewModel
        {
            get { return (TaskDetails) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
