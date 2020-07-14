using System;
using System.Collections.Generic;
using System.Linq;
using DataAPI.Client;
using DataAPI.DataStructures;
using DataExplorerWpf.ViewModels;
using Newtonsoft.Json.Linq;

namespace DataExplorerWpf.Visualization
{
    public class ImageVisualizationViewModelFactory : IDataVisualizationViewModelFactory
    {
        private readonly IDataApiClient dataApiClient;

        public ImageVisualizationViewModelFactory(IDataApiClient dataApiClient)
        {
            this.dataApiClient = dataApiClient;
        }

        public Type DataType { get; } = typeof(Image);

        public IDataVisualizationViewModel Create(IEnumerable<string> objects)
        {
            var images = objects.Select(JObject.Parse).Select(jObject => jObject.Value<string>("Id")).ToList();
            return new ImageVisualizationViewModel(dataApiClient, images);
        }
    }
}
