using System;
using DataAPI.DataStructures.DomainModels;
using Newtonsoft.Json;

namespace DataAPI.DataStructures.DataIo
{
    public class DataBlob : IId
    {
        [JsonConstructor]
        public DataBlob(string id, byte[] data, string filename = null)
        {
            Id = id ?? throw new ArgumentNullException();
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Filename = filename;
        }

        public string Id { get; private set; }
        public string Filename { get; private set; }
        public byte[] Data { get; private set; }
    }
}
