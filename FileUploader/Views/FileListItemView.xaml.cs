using System.Windows;
using System.Windows.Controls;
using SharedViewModels.ViewModels;

namespace FileUploader.Views
{
    /// <summary>
    /// Interaction logic for FileListItemView.xaml
    /// </summary>
    public partial class FileListItemView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(UploadedFileViewModel), typeof(FileListItemView), new PropertyMetadata(default(UploadedFileViewModel)));

        public FileListItemView()
        {
            InitializeComponent();
        }

        public UploadedFileViewModel ViewModel
        {
            get { return (UploadedFileViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
