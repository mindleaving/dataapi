using System;
using System.Collections.Generic;
using DataExplorerWpf.ViewModels;

namespace DataExplorerWpf.Visualization
{
    public interface IDataVisualizationViewModelFactory
    {
        Type DataType { get; }
        IDataVisualizationViewModel Create(IEnumerable<string> objects);
    }
}