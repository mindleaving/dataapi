using System.Collections.Generic;
using DataAPI.DataStructures.DomainModels;
using Newtonsoft.Json;

namespace DataServicesApp.Models
{
    public class DataServiceDefinition : IDataServiceDefinition<DataServiceDefinition.Field>
    {
        [JsonConstructor]
        public DataServiceDefinition(
            string id,
            string ownerInitials,
            string dataType,
            List<Field> fields,
            IDataServiceTarget target,
            string filter = null)
        {
            Id = id;
            OwnerInitials = ownerInitials;
            DataType = dataType;
            Fields = fields;
            Target = target;
            Filter = filter;
        }

        public string Id { get; }
        public string OwnerInitials { get; }
        public string DataType { get; }
        public string Filter { get; }
        public List<Field> Fields { get; }
        public IDataServiceTarget Target { get; }

        public class Field : IDataServiceDefinitionField
        {
            public Field(string path, string @as = null)
            {
                Path = path;
                As = @as;
            }

            public string Path { get; }
            public string As { get; }
        }
    }
}
