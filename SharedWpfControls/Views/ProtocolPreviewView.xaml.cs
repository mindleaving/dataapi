using System.Windows;
using System.Windows.Controls;
using DataAPI.DataStructures.DataManagement;

namespace SharedWpfControls.Views
{
    /// <summary>
    /// Interaction logic for ProtocolPreviewView.xaml
    /// </summary>
    public partial class ProtocolPreviewView : UserControl
    {
        public static readonly DependencyProperty ProtocolProperty = DependencyProperty.Register("Protocol", 
            typeof(DataCollectionProtocol), typeof(ProtocolPreviewView), new PropertyMetadata(default(DataCollectionProtocol)));

        public ProtocolPreviewView()
        {
            InitializeComponent();
        }

        public DataCollectionProtocol Protocol
        {
            get { return (DataCollectionProtocol) GetValue(ProtocolProperty); }
            set { SetValue(ProtocolProperty, value); }
        }
    }
}
