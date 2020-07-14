using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataAPI.Service.DataRouting;
using DataAPI.Service.DataStorage;
using Newtonsoft.Json.Linq;

namespace DataAPI.Service.Validators
{
    public class IsReferenceRuleEvaluator
    {
        private readonly IDataRouter dataRouter;

        public IsReferenceRuleEvaluator(IDataRouter dataRouter)
        {
            this.dataRouter = dataRouter;
        }

        public async Task<bool> CheckValidReferenceAsync(JToken jToken, Match regexMatch)
        {
            if (jToken == null)
                return false;
            var dataType = regexMatch.Groups[1].Value;
            var id = jToken.Value<string>();
            IRdDataStorage dataStorage;
            try
            {
                dataStorage = await dataRouter.GetSourceSystemAsync(dataType);
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
            return await dataStorage.ExistsAsync(dataType, id);
        }
    }
}
