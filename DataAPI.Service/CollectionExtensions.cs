using System.Collections.Generic;
using System.Linq;

namespace DataAPI.Service
{
    public static class CollectionExtensions
    {
        public static bool Equivalent<T>(this IList<T> list1, IList<T> list2)
        {
            var distinctList1 = list1.Distinct().ToList();
            var distinctList2 = list2.Distinct().ToList();
            if (distinctList1.Count != distinctList2.Count)
                return false;
            return distinctList1.Intersect(distinctList2).Count() == distinctList1.Count;
        }
    }
}
