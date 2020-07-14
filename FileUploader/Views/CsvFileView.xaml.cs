using System.Windows;
using System.Windows.Controls;
using SharedViewModels.ViewModels;

namespace FileUploader.Views
{
    /// <summary>
    /// Interaction logic for CsvFileView.xaml
    /// </summary>
    public partial class CsvFileView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(CsvFileViewModel), typeof(CsvFileView), new PropertyMetadata(default(CsvFileViewModel)));

        public CsvFileView()
        {
            InitializeComponent();
        }

        public CsvFileViewModel ViewModel
        {
            get { return (CsvFileViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
