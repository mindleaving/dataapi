using System;
using System.Windows;
using System.Windows.Controls;
using DataAPI.Client;
using SharedViewModels.Helpers;

namespace SharedWpfControls.Views
{
    /// <summary>
    /// Interaction logic for DataLinkButton.xaml
    /// </summary>
    public partial class DataLinkButton : UserControl
    {
        public static readonly DependencyProperty DataApiConfigurationProperty = DependencyProperty.Register("DataApiConfiguration", 
            typeof(ApiConfiguration), typeof(DataLinkButton), new PropertyMetadata(default(ApiConfiguration)));

        public static readonly DependencyProperty DataTypeProperty = DependencyProperty.Register("DataType", 
            typeof(string), typeof(DataLinkButton), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty IdProperty = DependencyProperty.Register("Id", 
            typeof(string), typeof(DataLinkButton), new PropertyMetadata(default(string)));

        public DataLinkButton()
        {
            InitializeComponent();
        }

        public ApiConfiguration DataApiConfiguration
        {
            get { return (ApiConfiguration) GetValue(DataApiConfigurationProperty); }
            set { SetValue(DataApiConfigurationProperty, value); }
        }

        public string DataType
        {
            get { return (string) GetValue(DataTypeProperty); }
            set { SetValue(DataTypeProperty, value); }
        }

        public string Id
        {
            get { return (string) GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }

        private void CopyDataLink(object sender, RoutedEventArgs e)
        {
            if(DataApiConfiguration == null)
            {
                MessageBox.Show($"BUG: {nameof(DataApiConfiguration)} isn't set for {nameof(DataLinkButton)}");
                return;
            }
            if (DataType == null || Id == null)
            {
                MessageBox.Show("Invalid data. This is probably a bug. Contact the application owner.");
                return;
            }
            var query = $"dataType={Uri.EscapeDataString(DataType)}&id={Uri.EscapeDataString(Id)}";
            var serverAddress = DataApiConfiguration.ServerAddress;
            if (DataApiConfiguration.ServerPort != 443)
                serverAddress += $":{DataApiConfiguration.ServerPort}";
            var link = $"https://{serverAddress}/download/getfile?{query}";
            Clipboard.SetText(link);
            StaticMessageBoxSpawner.Show("Link copied to clipboard");
        }
    }
}
