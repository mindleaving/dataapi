using System;
using System.IO;
using System.Threading.Tasks;

namespace DataAPI.Service.DataStorage
{
    public interface IBlob
    {
        DateTime CreatedTimestampUtc { get; }
        DateTime LastModifiedTimestampUtc { get; }
        Task<bool> ExistsAsync();
        Stream GetStream();
        Task WriteAsync(byte[] bytes);
        Task WriteAsync(Stream stream);
        Task DeleteAsync();
    }
}