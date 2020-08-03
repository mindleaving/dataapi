using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DataAPI.Client;
using DataAPI.DataStructures.DataManagement;
using SharedViewModels.Helpers;

namespace SharedWpfControls.Views
{
    /// <summary>
    /// Interaction logic for ShortIdView.xaml
    /// </summary>
    public partial class ShortIdView : UserControl
    {
        public static readonly DependencyProperty DataApiConfigurationProperty = DependencyProperty.Register("DataApiConfiguration", 
            typeof(ApiConfiguration), typeof(ShortIdView), new PropertyMetadata(default(ApiConfiguration)));

        public static readonly DependencyProperty ShortIdProperty = DependencyProperty.Register("ShortId", 
            typeof(ShortId), typeof(ShortIdView), new PropertyMetadata(default(ShortId)));

        public ShortIdView()
        {
            InitializeComponent();
        }

        public ApiConfiguration DataApiConfiguration
        {
            get { return (ApiConfiguration) GetValue(DataApiConfigurationProperty); }
            set { SetValue(DataApiConfigurationProperty, value); }
        }

        public ShortId ShortId
        {
            get { return (ShortId) GetValue(ShortIdProperty); }
            set { SetValue(ShortIdProperty, value); }
        }

        private void ShortIdClicked(object sender, MouseButtonEventArgs e)
        {
            if (DataApiConfiguration == null)
            {
                MessageBox.Show($"BUG: {nameof(DataApiConfiguration)} isn't set for {nameof(ShortIdView)}");
                return;
            }
            var border = sender as Border;
            if(border == null)
                return;
            var stackPanel = border.Child as StackPanel;
            if(stackPanel == null)
                return;
            var textBlock = stackPanel.Children.OfType<TextBlock>().SingleOrDefault();
            if(textBlock == null)
                return;
            var id = textBlock.Text;
            var query = $"dataType={Uri.EscapeDataString(ShortId.CollectionName)}&id={Uri.EscapeDataString(ShortId.OriginalId)}";
            var serverAddress = DataApiConfiguration.ServerAddress;
            if (DataApiConfiguration.ServerPort != 443)
                serverAddress += $":{DataApiConfiguration.ServerPort}";
            Clipboard.SetText($"https://{serverAddress}/download/getfile?{query}");
            StaticMessageBoxSpawner.Show($"Short ID '{id}' copied to clipboard");
        }
    }
}
