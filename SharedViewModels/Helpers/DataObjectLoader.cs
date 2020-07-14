using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using DataAPI.Client;
using DataAPI.DataStructures.DataManagement;
using DataAPI.DataStructures.Exceptions;
using Newtonsoft.Json.Linq;
using SharedViewModels.Objects;
using SharedViewModels.ViewModels;

namespace SharedViewModels.Helpers
{
    public class DataObjectLoader
    {
        private readonly IDataApiClient dataApiClient;
        private readonly JsonViewModelFactory jsonViewModelFactory;

        public DataObjectLoader(
            IDataApiClient dataApiClient,
            IClipboard clipboard,
            ICollectionSwitcher collectionSwitcher)
        {
            this.dataApiClient = dataApiClient;
            jsonViewModelFactory = new JsonViewModelFactory(clipboard, collectionSwitcher);
        }

        public async Task<List<DataObjectViewModel>> Load(
            IEnumerable<DataReference> dataReferences,
            Action<DataObjectViewModel> deletionCallback)
        {
            var dataObjects = new List<DataObjectViewModel>();
            foreach (var dataReference in dataReferences)
            {
                try
                {
                    var json = await dataApiClient.GetAsync(
                        dataReference.DataType,
                        dataReference.Id);
                    var dataObject = new DataObjectViewModel(
                        new JObjectViewModel(JObject.Parse(json), jsonViewModelFactory), 
                        dataReference.DataType, 
                        dataReference.Id, 
                        dataApiClient, 
                         
                        deletionCallback);
                    dataObjects.Add(dataObject);
                }
                catch (ApiException apiException)
                {
                    if(apiException.StatusCode == HttpStatusCode.NotFound)
                        continue; // Ignore
                    throw;
                }
            }

            return dataObjects;
        }
    }
}
