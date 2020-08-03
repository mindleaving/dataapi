using System.Windows;
using FileUploader.ViewModel;
using SharedViewModels.ViewModels;

namespace FileUploader.Views
{
    /// <summary>
    /// Interaction logic for AdditionalInformationWindow.xaml
    /// </summary>
    public partial class AdditionalInformationWindow : Window, IClosable
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(AdditionalInformationViewModel), typeof(AdditionalInformationWindow), new PropertyMetadata(default(AdditionalInformationViewModel)));

        public AdditionalInformationWindow()
        {
            InitializeComponent();
        }

        public AdditionalInformationViewModel ViewModel
        {
            get { return (AdditionalInformationViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public void Close(bool? dialogResult)
        {
            DialogResult = dialogResult;
            Close();
        }
    }
}
