using System.Windows;
using System.Windows.Controls;
using DataExplorerWpf.ViewModels;

namespace DataExplorerWpf.Views
{
    /// <summary>
    /// Interaction logic for ImageVisualizationView.xaml
    /// </summary>
    public partial class ImageVisualizationView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(ImageVisualizationViewModel), typeof(ImageVisualizationView), new PropertyMetadata(default(ImageVisualizationViewModel)));

        public ImageVisualizationView()
        {
            InitializeComponent();
        }

        public ImageVisualizationViewModel ViewModel
        {
            get { return (ImageVisualizationViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
