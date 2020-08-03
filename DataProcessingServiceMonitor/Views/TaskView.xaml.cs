using System.Windows;
using System.Windows.Controls;
using DataProcessingServiceMonitor.ViewModels;

namespace DataProcessingServiceMonitor.Views
{
    /// <summary>
    /// Interaction logic for TaskView.xaml
    /// </summary>
    public partial class TaskView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(TaskViewModel), typeof(TaskView), new PropertyMetadata(default(TaskViewModel)));

        public TaskView()
        {
            InitializeComponent();
        }

        public TaskViewModel ViewModel
        {
            get { return (TaskViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
