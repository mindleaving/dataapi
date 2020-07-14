using System.Windows;
using System.Windows.Controls;
using SharedViewModels.ViewModels;

namespace SharedWpfControls.Views
{
    /// <summary>
    /// Interaction logic for UnitValueEditView.xaml
    /// </summary>
    public partial class UnitValueEditView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(UnitValueEditViewModel), typeof(UnitValueEditView), new PropertyMetadata(default(UnitValueEditViewModel)));

        public UnitValueEditView()
        {
            InitializeComponent();
        }

        public UnitValueEditViewModel ViewModel
        {
            get { return (UnitValueEditViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
