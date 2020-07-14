using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DataExplorerWpf.ViewModels;
using SharedViewModels.ViewModels;

namespace DataExplorerWpf.Views
{
    /// <summary>
    /// Interaction logic for DataWindow.xaml
    /// </summary>
    public partial class DataWindow : Window
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(DataWindowViewModel), typeof(DataWindow), new PropertyMetadata(default(DataWindowViewModel)));

        public DataWindow()
        {
            InitializeComponent();
        }

        public DataWindowViewModel ViewModel
        {
            get { return (DataWindowViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        private void DataObjectList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItems = DataObjectList.SelectedItems.OfType<DataObjectViewModel>().ToList();
            var newItems = selectedItems.Except(ViewModel.SelectedDataObjects).ToList();
            var removedItems = ViewModel.SelectedDataObjects.Except(selectedItems).ToList();
            foreach (var dataObject in removedItems)
            {
                ViewModel.SelectedDataObjects.Remove(dataObject);
            }

            foreach (var dataObject in newItems)
            {
                ViewModel.SelectedDataObjects.Add(dataObject);
            }
        }
    }
}
