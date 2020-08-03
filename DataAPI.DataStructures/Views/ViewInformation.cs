using Newtonsoft.Json;

namespace DataAPI.DataStructures.Views
{
    public class ViewInformation
    {
        [JsonConstructor]
        public ViewInformation(string viewId)
        {
            ViewId = viewId;
        }

        public string ViewId { get; }
    }
}
