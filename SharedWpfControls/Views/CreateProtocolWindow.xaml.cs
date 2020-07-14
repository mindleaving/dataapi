using System.Windows;
using SharedViewModels.ViewModels;

namespace SharedWpfControls.Views
{
    /// <summary>
    /// Interaction logic for CreateProtocolWindow.xaml
    /// </summary>
    public partial class CreateProtocolWindow : Window, IClosable
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(CreateProtocolViewModel), typeof(CreateProtocolWindow), new PropertyMetadata(default(CreateProtocolViewModel)));

        public CreateProtocolWindow()
        {
            InitializeComponent();
        }

        public CreateProtocolViewModel ViewModel
        {
            get { return (CreateProtocolViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public void Close(bool? dialogResult)
        {
            DialogResult = dialogResult;
            Close();
        }
    }
}
