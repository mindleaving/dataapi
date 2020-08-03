using System.Windows;

namespace SharedWpfControls.Views
{
    /// <summary>
    /// Interaction logic for StartupWindow.xaml
    /// </summary>
    public partial class StartupWindow : Window
    {
        public static readonly DependencyProperty ApplicationNameProperty = DependencyProperty.Register("ApplicationName", 
            typeof(string), typeof(StartupWindow), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message", 
            typeof(string), typeof(StartupWindow), new PropertyMetadata(default(string)));

        public StartupWindow()
        {
            InitializeComponent();
        }

        public string ApplicationName
        {
            get { return (string) GetValue(ApplicationNameProperty); }
            set { SetValue(ApplicationNameProperty, value); }
        }

        public string Message
        {
            get { return (string) GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }
    }
}
