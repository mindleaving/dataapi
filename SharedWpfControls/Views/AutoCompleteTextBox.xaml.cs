using System.Windows;
using System.Windows.Controls;
using SharedViewModels.ViewModels;

namespace SharedWpfControls.Views
{
    /// <summary>
    /// Interaction logic for AutoCompleteTextBox.xaml
    /// </summary>
    public partial class AutoCompleteTextBox : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(IAutoCompleteTextBoxViewModel), typeof(AutoCompleteTextBox), new PropertyMetadata(default(IAutoCompleteTextBoxViewModel)));

        public AutoCompleteTextBox()
        {
            InitializeComponent();
        }

        public IAutoCompleteTextBoxViewModel ViewModel
        {
            get { return (IAutoCompleteTextBoxViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
