using System.Threading.Tasks;

namespace DataAPI.Client.Repositories
{
    public interface IObjectDatabase<T> : IReadonlyObjectDatabase<T>
    {
        Task StoreAsync(T obj);
        Task DeleteAsync(string id);
    }
}