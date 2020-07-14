using System.Windows;
using System.Windows.Controls;
using SharedViewModels.ViewModels;

namespace SharedWpfControls.Views
{
    /// <summary>
    /// Interaction logic for ShortIdView.xaml
    /// </summary>
    public partial class ShortIdEditView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(ShortIdEditViewModel), typeof(ShortIdEditView), new PropertyMetadata(default(ShortIdEditViewModel)));

        public ShortIdEditView()
        {
            InitializeComponent();
        }

        public ShortIdEditViewModel ViewModel
        {
            get { return (ShortIdEditViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
