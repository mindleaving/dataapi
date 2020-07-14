using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace SharedViewModels.ViewModels
{
    public interface IJsonViewModel
    {
        JToken JToken { get; }
        List<IJsonViewModel> Children { get; }
    }
}
