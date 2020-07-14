using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DataAPI.Service.IdGeneration;

namespace DataAPI.Service.Test.DataStorage
{
    internal class DummyIdGeneratorManager : IIdGeneratorManager
    {
        private readonly IIdGenerator idGenerator = new GuidIdGenerator();

        public Task<List<string>> GetIdsAsync(string dataType, int count, CancellationToken cancellationToken = default)
        {
            return idGenerator.GetIdsAsync(dataType, count, cancellationToken);
        }

        public Task<IIdGenerator> GetIdGeneratorForDataTypeAsync(string dataType)
        {
            return Task.FromResult(idGenerator);
        }
    }
}