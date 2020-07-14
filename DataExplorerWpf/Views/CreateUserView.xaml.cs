using System.Windows;
using System.Windows.Controls;
using DataExplorerWpf.ViewModels;

namespace DataExplorerWpf.Views
{
    /// <summary>
    /// Interaction logic for CreateUserView.xaml
    /// </summary>
    public partial class CreateUserView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(CreateUserViewModel), typeof(CreateUserView), new PropertyMetadata(default(CreateUserViewModel)));

        public CreateUserView()
        {
            InitializeComponent();
        }

        public CreateUserViewModel ViewModel
        {
            get { return (CreateUserViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
