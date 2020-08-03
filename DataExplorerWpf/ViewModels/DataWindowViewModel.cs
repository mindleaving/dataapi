using System.Collections.Generic;
using System.Collections.ObjectModel;
using SharedViewModels.ViewModels;

namespace DataExplorerWpf.ViewModels
{
    public class DataWindowViewModel : NotifyPropertyChangedBase
    {
        public DataWindowViewModel(string windowTitle, List<DataObjectViewModel> dataObjects)
        {
            WindowTitle = windowTitle;
            DataObjects = dataObjects;
            SelectedDataObjects = new ObservableCollection<DataObjectViewModel>();
        }

        public List<DataObjectViewModel> DataObjects { get; }
        public ObservableCollection<DataObjectViewModel> SelectedDataObjects { get; }
        public string WindowTitle { get; }
    }
}
