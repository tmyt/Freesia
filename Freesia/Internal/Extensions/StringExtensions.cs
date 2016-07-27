using System;

namespace Freesia.Internal.Extensions
{
    internal static class StringExtensions
    {
        public static bool CompareIgnoreCaseTo(this string lhs, string rhs)
        {
            return string.Compare(lhs, rhs, StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}
