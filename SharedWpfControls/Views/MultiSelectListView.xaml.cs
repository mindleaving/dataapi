using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SharedWpfControls.Views
{
    /// <summary>
    /// Interaction logic for MultiSelectListView.xaml
    /// </summary>
    public partial class MultiSelectListView : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", 
            typeof(IEnumerable), typeof(MultiSelectListView), new PropertyMetadata(default(IEnumerable)));

        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register("SelectedItems", 
            typeof(IList), typeof(MultiSelectListView), new FrameworkPropertyMetadata
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

        public MultiSelectListView()
        {
            InitializeComponent();
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable) GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public IList SelectedItems
        {
            get { return (IList) GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }


        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItems = ListView.SelectedItems;
        }
    }
}