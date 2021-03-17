using System.Linq;
using System.Collections.Generic;


namespace Drawmasters
{
    public static class CommonExtensions
    {
        public static IEnumerable<T> TakeMax<T>(this IEnumerable<T> coll, int N) =>
            coll.Count() <= N ? coll : coll.Take(N);


        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> coll, int N) =>
            coll.Reverse().Take(N).Reverse();

        public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> coll, int N) =>
            coll.Reverse().Skip(N).Reverse();
    }
}
