using System;
using System.ComponentModel.DataAnnotations;

namespace DataAPI.DataStructures.DomainModels
{
    public interface IDataReference : IEquatable<IDataReference>
    {
        [Required]
        string DataType { get; }

        [Required]
        string Id { get; }
    }
}