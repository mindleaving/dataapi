using System.Windows;
using SharedViewModels.ViewModels;

namespace SharedWpfControls.Views
{
    /// <summary>
    /// Interaction logic for CreateProjectWindow.xaml
    /// </summary>
    public partial class CreateProjectWindow : Window, IClosable
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(CreateProjectViewModel), typeof(CreateProjectWindow), new PropertyMetadata(default(CreateProjectViewModel)));

        public CreateProjectWindow()
        {
            InitializeComponent();
        }

        public CreateProjectViewModel ViewModel
        {
            get { return (CreateProjectViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public void Close(bool? dialogResult)
        {
            DialogResult = dialogResult;
            Close();
        }
    }
}
