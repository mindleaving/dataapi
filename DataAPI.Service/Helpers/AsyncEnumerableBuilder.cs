using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAPI.Service.Helpers
{
    public static class AsyncEnumerableBuilder
    {
        public static IAsyncEnumerable<T> FromArray<T>(IList<T> items)
        {
            var index = -1;
            return AsyncEnumerable.Create<T>(
                cancellationToken => AsyncEnumerator.Create(
                    () =>
                    {
                        if(index+1 >= items.Count)
                            return new ValueTask<bool>(false);
                        index++;
                        return new ValueTask<bool>(true);
                    },
                    () => items[index],
                    () => new ValueTask()));
        }
    }
}