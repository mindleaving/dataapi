using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DataAPI.DataStructures.Attributes;
using DataAPI.DataStructures.Validation;

namespace DataAPI.DataStructures.DataManagement
{
    [DataApiCollection("DataProjectUploadInfo")]
    public interface IDataProjectUploadInfo<TDataReference> : IId
        where TDataReference: IDataReference
    {
        [Required]
        string UploaderInitials { get; }

        [Required]
        DateTime UploadTimestamp { get; }

        [Required]
        [DataReference("DataProject")]
        string DataProjectId { get; }

        string Filename { get; }

        [Required]
        TDataReference RawData { get; }

        List<TDataReference> DerivedData { get; }
    }
}