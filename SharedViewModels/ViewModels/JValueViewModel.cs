using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace SharedViewModels.ViewModels
{
    public class JValueViewModel : IJsonViewModel
    {
        public JValueViewModel(object value)
        {
            JToken = null;
            Value = value;
            Children = new List<IJsonViewModel>();
        }

        public JToken JToken { get; }
        public object Value { get; }
        public List<IJsonViewModel> Children { get; }
    }
}
