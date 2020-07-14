using System;
using System.Linq;
using System.Threading.Tasks;
using DataAPI.DataStructures.PostBodies;
using DataAPI.Service.DataStorage;
using Newtonsoft.Json.Linq;

namespace DataAPI.Service.IdGeneration
{
    public class DefaultIdPolicy : IIdPolicy
    {
        public async Task<SuggestedId> DetermineIdAsync(SubmitDataBody body, string submitter, IRdDataStorage rdDataStorage)
        {
            var dataType = body.Data.GetType();
            string dataId;
            if (dataType == typeof(JObject))
            {
                var jObject = (JObject)body.Data;
                dataId = jObject["Id"]?.Value<string>()
                         ?? jObject["id"]?.Value<string>()
                         ?? jObject["ID"]?.Value<string>()
                         ?? jObject["_id"]?.Value<string>();
            }
            else
            {
                var idProperty = dataType.GetProperty("Id")
                             ?? dataType.GetProperty("id")
                             ?? dataType.GetProperty("ID")
                             ?? dataType.GetProperty("_id");
                dataId = idProperty?.GetValue(body.Data) as string;
            }

            if (dataId != null)
                return new SuggestedId(dataId, false);
            if (body.Id != null)
                return new SuggestedId(body.Id, false);
            var idReservationResult = (await rdDataStorage.GetIdsAsync(body.DataType, submitter, 1)).Single();
            if(!idReservationResult.IsReserved)
                throw new Exception($"Could not determine a suitable ID for data type '{dataType}'");
            return new SuggestedId(idReservationResult.Id, idReservationResult.IsReserved);
        }
    }
}
