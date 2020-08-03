using DataAPI.DataStructures.Attributes;

namespace DataAPI.DataStructures.DataIo
{
    [DataApiCollection("DataBlob")]
    public interface IDataBlob : IId
    {
        string Filename { get; }
        byte[] Data { get; }
    }
}