using System;
using System.Collections.Generic;
using System.Linq;

namespace Freesia.Internal.Extensions
{
    internal static class EnumerableEx
    {
        internal class EqualityComparer<T>: IEqualityComparer<T>
        {
            private Func<T, T, bool> _comparer;

            public EqualityComparer(Func<T, T, bool> comparer)
            {
                _comparer = comparer;
            }

            public bool Equals(T x, T y)
            {
                return _comparer(x, y);
            }

            public int GetHashCode(T obj)
            {
                return obj.GetHashCode();
            }
        }

        public static bool Contains<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return source.Any(predicate);
        }

        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value)
        {
            return source.Any(x => x.Equals(value));
        }

        public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source,
            Func<TSource, TSource, bool> predicate)
        {
            return source.Distinct(new EqualityComparer<TSource>(predicate));
        }
    }
}
