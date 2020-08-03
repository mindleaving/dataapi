using DataAPI.DataStructures.Attributes;

namespace DataAPI.DataStructures.DomainModels
{
    [DataApiCollection("DataBlob")]
    public interface IDataBlob : IId
    {
        string Filename { get; }
        byte[] Data { get; }
    }
}