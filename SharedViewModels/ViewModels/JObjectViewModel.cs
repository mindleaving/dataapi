using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace SharedViewModels.ViewModels
{
    public class JObjectViewModel : IJsonViewModel
    {
        public JObjectViewModel(JObject jObject, JsonViewModelFactory jsonViewModelFactory)
        {
            JToken = jObject;
            var id = jObject.Properties().SingleOrDefault(x => x.Name.ToLowerInvariant() == "id")?.Value.ToString()
                     ?? jObject.Properties().SingleOrDefault(x => x.Name.ToLowerInvariant() == "_id")?.Value.ToString()
                     ?? jObject.Properties().FirstOrDefault(x => x.Name.EndsWith("Id"))?.Value.ToString()
                     ?? "(no ID)";
            Name = id;
            Children = jObject.Properties()
                .Select(jsonViewModelFactory.Create)
                .ToList();
        }

        public JToken JToken { get; }
        public string Name { get; }
        public List<IJsonViewModel> Children { get; }
    }
}
