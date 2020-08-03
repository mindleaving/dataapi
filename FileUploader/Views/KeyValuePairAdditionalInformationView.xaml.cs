using System.Windows;
using System.Windows.Controls;
using FileHandlers.AdditionalInformation;

namespace FileUploader.Views
{
    /// <summary>
    /// Interaction logic for KeyValuePairAdditionalInformationView.xaml
    /// </summary>
    public partial class KeyValuePairAdditionalInformationView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(KeyValuePairAdditionalInformationViewModel), typeof(KeyValuePairAdditionalInformationView), new PropertyMetadata(default(KeyValuePairAdditionalInformationViewModel)));

        public KeyValuePairAdditionalInformationView()
        {
            InitializeComponent();
        }

        public KeyValuePairAdditionalInformationViewModel ViewModel
        {
            get { return (KeyValuePairAdditionalInformationViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
