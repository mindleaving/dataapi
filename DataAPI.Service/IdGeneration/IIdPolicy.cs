using System.Threading.Tasks;
using DataAPI.DataStructures.PostBodies;
using DataAPI.Service.DataStorage;

namespace DataAPI.Service.IdGeneration
{
    public interface IIdPolicy
    {
        Task<SuggestedId> DetermineIdAsync(SubmitDataBody body, string submitter, IRdDataStorage rdDataStorage);
    }
}