using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DataAPI.Service.IdGeneration
{
    public class GuidIdGenerator : IIdGenerator
    {
        public Task<List<string>> GetIdsAsync(string dataType, int count, CancellationToken cancellationToken = default)
        {
            var ids = new List<string>();
            for (int i = 0; i < count; i++)
            {
                ids.Add(Guid.NewGuid().ToString());
            }
            return Task.FromResult(ids);
        }
    }
}
