using System.Windows;
using DataExplorerWpf.ViewModels;
using SharedViewModels.ViewModels;

namespace DataExplorerWpf.Views
{
    /// <summary>
    /// Interaction logic for CollectionSettingsWindow.xaml
    /// </summary>
    public partial class CollectionSettingsWindow : Window, IClosable
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(CollectionSettingsViewModel), typeof(CollectionSettingsWindow), new PropertyMetadata(default(CollectionSettingsViewModel)));

        public CollectionSettingsWindow()
        {
            InitializeComponent();
        }

        public CollectionSettingsViewModel ViewModel
        {
            get { return (CollectionSettingsViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public void Close(bool? dialogResult)
        {
            DialogResult = dialogResult;
            Close();
        }
    }
}
