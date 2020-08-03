using System.Windows;
using System.Windows.Controls;

namespace SharedWpfControls.Views
{
    /// <summary>
    /// Interaction logic for DataCollectionProtocolParameterValueView.xaml
    /// </summary>
    public partial class DataCollectionProtocolParameterValueView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(object), 
            typeof(DataCollectionProtocolParameterValueView), 
            new PropertyMetadata(default(object)));

        public DataCollectionProtocolParameterValueView()
        {
            InitializeComponent();
        }

        public object ViewModel
        {
            get { return GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
