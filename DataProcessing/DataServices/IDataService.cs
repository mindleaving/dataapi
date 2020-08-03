using System.Collections.Generic;
using System.Threading.Tasks;
using DataProcessing.Models;
using Newtonsoft.Json.Linq;

namespace DataProcessing.DataServices
{
    public interface IDataService
    {
        Task InitializeAsync();
        Task TransferAsync(JObject jObject, List<DataServiceDefinition.Field> fields, string id);
        Task DeleteAsync(string id);
        Task<bool> ObjectExistsAsync(string id);
    }
}