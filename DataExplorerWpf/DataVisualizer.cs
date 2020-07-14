using System.Collections.Generic;
using System.Linq;
using DataExplorerWpf.Views;
using DataExplorerWpf.Visualization;

namespace DataExplorerWpf
{
    public class DataVisualizer
    {
        private readonly Dictionary<string, IDataVisualizationViewModelFactory> viewModelFactories;

        public DataVisualizer(IEnumerable<IDataVisualizationViewModelFactory> viewModels)
        {
            viewModelFactories = viewModels.ToDictionary(vm => vm.DataType.Name, vm => vm);
        }

        public bool CanVisualize(string typeName)
        {
            return viewModelFactories.ContainsKey(typeName);
        }

        public void Visualize(string typeName, IEnumerable<string> objects)
        {
            var factory = viewModelFactories[typeName];
            var viewModel = factory.Create(objects);
            var dataVisualizationWindow = new DataVisualizationWindow
            {
                ViewModel = viewModel
            };
            dataVisualizationWindow.ShowDialog();
        }
    }
}
