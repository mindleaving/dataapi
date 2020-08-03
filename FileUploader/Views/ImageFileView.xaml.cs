using System.Windows;
using System.Windows.Controls;
using SharedViewModels.ViewModels;

namespace FileUploader.Views
{
    /// <summary>
    /// Interaction logic for ImageFileView.xaml
    /// </summary>
    public partial class ImageFileView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(ImageFileViewModel), typeof(ImageFileView), new PropertyMetadata(default(ImageFileViewModel)));

        public ImageFileView()
        {
            InitializeComponent();
        }

        public ImageFileViewModel ViewModel
        {
            get { return (ImageFileViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
