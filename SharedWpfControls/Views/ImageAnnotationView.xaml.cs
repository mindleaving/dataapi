using System.Windows;
using System.Windows.Controls;
using SharedWpfControls.ViewModels;

namespace SharedWpfControls.Views
{
    /// <summary>
    /// Interaction logic for ImageAnnotationView.xaml
    /// </summary>
    public partial class ImageAnnotationView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(ImageAnnotationViewModel), typeof(ImageAnnotationView), new PropertyMetadata(default(ImageAnnotationViewModel)));

        public ImageAnnotationView()
        {
            InitializeComponent();
        }

        public ImageAnnotationViewModel ViewModel
        {
            get { return (ImageAnnotationViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
