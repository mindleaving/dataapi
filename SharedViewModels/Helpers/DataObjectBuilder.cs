using System;
using System.Collections.Generic;
using DataAPI.Client;
using Newtonsoft.Json.Linq;
using SharedViewModels.Objects;
using SharedViewModels.ViewModels;

namespace SharedViewModels.Helpers
{
    public static class DataObjectBuilder
    {
        public static List<DataObjectViewModel> BuildItems(
            List<JObject> searchResults,
            bool includeMetadata,
            string dataType,
            IDataApiClient dataApiClient,
            IClipboard clipboard,
            ICollectionSwitcher collectionSwitcher,
            Action<DataObjectViewModel> deleteCallback)
        {
            var dataObjects = new List<DataObjectViewModel>();
            var jsonViewModelFactory = new JsonViewModelFactory(clipboard, collectionSwitcher);
            foreach (var jObject in searchResults)
            {
                var id = jObject.ContainsKey("_id") ? jObject["_id"].Value<string>() : null;
                JToken data = jObject;
                if(!includeMetadata && jObject.ContainsKey("Data"))
                    data = jObject["Data"];
                if(!(jsonViewModelFactory.Create(data) is JObjectViewModel jObjectViewModel))
                    continue;
                var dataObject = new DataObjectViewModel(jObjectViewModel, dataType, id, dataApiClient, deleteCallback);
                dataObjects.Add(dataObject);
            }
            return dataObjects;
        }
    }
}
