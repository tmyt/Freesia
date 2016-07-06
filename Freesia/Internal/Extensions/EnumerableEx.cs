using System;
using System.Collections.Generic;
using System.Linq;

namespace Freesia.Internal.Extensions
{
    internal static class EnumerableEx
    {
        public static bool Contains<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return source.FirstOrDefault(predicate) != null;
        }
    }
}
