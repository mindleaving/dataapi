using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DataProcessingServiceMonitor.ViewModels;

namespace DataProcessingServiceMonitor.Views
{
    /// <summary>
    /// Interaction logic for LogEntryList.xaml
    /// </summary>
    public partial class LogEntryListView : UserControl
    {
        public static readonly DependencyProperty MessagesProperty = DependencyProperty.Register("Messages", 
            typeof(ICollection<LogEntryViewModel>), typeof(LogEntryListView), new PropertyMetadata(default(ICollection<LogEntryViewModel>)));

        public LogEntryListView()
        {
            InitializeComponent();
        }

        public ICollection<LogEntryViewModel> Messages
        {
            get { return (ICollection<LogEntryViewModel>) GetValue(MessagesProperty); }
            set { SetValue(MessagesProperty, value); }
        }
    }
}
