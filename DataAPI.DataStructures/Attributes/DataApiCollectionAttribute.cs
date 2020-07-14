using System;

namespace DataAPI.DataStructures.Attributes
{
    public class DataApiCollectionAttribute : Attribute
    {
        public string CollectionName { get; }

        public DataApiCollectionAttribute(string collectionName)
        {
            CollectionName = collectionName;
        }
    }
}
