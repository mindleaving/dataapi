using System.Collections.Generic;

namespace DataAPI.Service.DataStorage
{
    public interface IBinaryDataStorage
    {
        IEnumerable<string> ListContainers();
        IBlobContainer GetContainer(string dataType);
        IBlob GetBlob(string dataType, string id);
    }
}
