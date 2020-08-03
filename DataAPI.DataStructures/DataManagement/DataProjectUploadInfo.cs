using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace DataAPI.DataStructures.DataManagement
{
    public class DataProjectUploadInfo : IDataProjectUploadInfo<DataReference>
    {
        [JsonConstructor]
        private DataProjectUploadInfo(
            string id, 
            string uploaderInitials, 
            DateTime uploadTimestamp, 
            string dataProjectId, 
            DataReference rawData,
            List<DataReference> derivedData,
            string filename)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            UploadTimestamp = uploadTimestamp;
            DataProjectId = dataProjectId ?? throw new ArgumentNullException(nameof(dataProjectId));
            RawData = rawData ?? throw new ArgumentNullException(nameof(rawData));
            DerivedData = derivedData ?? new List<DataReference>();
            Filename = filename;
            UploaderInitials = uploaderInitials;
        }

        public DataProjectUploadInfo(
            string uploaderInitials, 
            DateTime uploadTimestamp, 
            string dataProjectId, 
            DataReference rawData,
            List<DataReference> derivedData,
            string filename = null)
            : this(IdGenerator.FromGuid(), uploaderInitials, uploadTimestamp, dataProjectId, rawData, derivedData, filename)
        {
        }

        [Required]
        public string Id { get; private set; }
        [Required]
        public string UploaderInitials { get; private set; }
        [Required]
        public DateTime UploadTimestamp { get; private set; }
        [Required]
        public string DataProjectId { get; private set; }
        public string Filename { get; private set; }
        [Required]
        public DataReference RawData { get; private set; }
        public List<DataReference> DerivedData { get; private set; }
    }
}
