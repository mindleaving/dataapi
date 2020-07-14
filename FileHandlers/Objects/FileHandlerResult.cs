using System.Collections.Generic;
using DataAPI.DataStructures.DataManagement;
using SharedViewModels.ViewModels;

namespace FileHandlers.Objects
{
    public class FileHandlerResult
    {
        public FileHandlerResult(List<DataReference> derivedDataReferences, List<IDerivedFileDataViewModel> viewModels)
        {
            DerivedDataReferences = derivedDataReferences;
            ViewModels = viewModels;
        }

        public List<DataReference> DerivedDataReferences { get; }
        public List<IDerivedFileDataViewModel> ViewModels { get; }
    }
}