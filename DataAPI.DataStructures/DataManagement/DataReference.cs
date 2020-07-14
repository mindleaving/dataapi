using System;
using System.ComponentModel.DataAnnotations;
using DataAPI.DataStructures.DomainModels;
using Newtonsoft.Json;

namespace DataAPI.DataStructures.DataManagement
{
    public class DataReference : IDataReference
    {
        [JsonConstructor]
        public DataReference(string dataType, string id)
        {
            DataType = dataType ?? throw new ArgumentNullException(nameof(dataType));
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }

        public static DataReference FromIId(IId obj)
        {
            return new DataReference(obj.GetType().Name, obj.Id);
        }

        [Required]
        public string DataType { get; private set; }
        [Required]
        public string Id { get; private set; }

        public bool Equals(IDataReference other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(DataType, other.DataType) && string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((DataReference) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (DataType.GetHashCode() * 397) ^ Id.GetHashCode();
            }
        }

        public static bool operator ==(DataReference left, DataReference right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DataReference left, DataReference right)
        {
            return !Equals(left, right);
        }
    }
}