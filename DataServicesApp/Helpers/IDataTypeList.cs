using System.Collections.Generic;
using DataAPI.DataStructures;

namespace DataServicesApp.Helpers
{
    public interface IDataTypeList
    {
        List<CollectionInformation> GetCollections();
    }
}