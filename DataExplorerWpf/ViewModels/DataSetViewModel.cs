using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAPI.Client;
using DataAPI.DataStructures.DataManagement;
using SharedViewModels.ViewModels;

namespace DataExplorerWpf.ViewModels
{
    public class DataSetViewModel : NotifyPropertyChangedBase
    {
        private readonly IDataApiClient dataApiClient;

        public DataSetViewModel(DataSet model, IDataApiClient dataApiClient)
        {
            this.dataApiClient = dataApiClient;
            
            Model = model;
        }

        public DataSet Model { get; }

        public ApiConfiguration DataApiConfiguration => dataApiClient.ApiConfiguration;
        private List<DataReferenceViewModel> dataReferences;
        public List<DataReferenceViewModel> DataReferences => dataReferences ?? (dataReferences = LoadDataReferences());

        private List<DataReferenceViewModel> LoadDataReferences()
        {
            var result = Task.Run(
                () =>
                {
                    var taggedData = dataApiClient.GetManyAsync<DataTag>(
                        $"Data.{nameof(DataTag.TagName)} = '{Model.Id}'").Result;
                    return taggedData
                        .Select(tag => tag.DataReference)
                        .Select(dataReference => new DataReferenceViewModel(dataReference, dataApiClient))
                        .ToList();
                }).Result;
            return result;
        }
    }
}
