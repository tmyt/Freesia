using System;
using System.Collections.Generic;
using System.Linq;
using Freesia.Types;

namespace Freesia
{
    public class CompilerConfig<T>
    {
        protected class UserFunctionTypePlaceholder { }

        protected static IEnumerable<CompletionResult> UserFunctionNamespaces()
        {
            if (string.IsNullOrEmpty(UserFunctionNamespace)) yield break;
            yield return new CompletionResult(MemberType.Property, "???", UserFunctionNamespace);
        }

        protected static IEnumerable<CompletionResult> UserFunctionKeys()
        {
            return Functions.Keys.Select(x => CompletionResult.Property(typeof(bool), x));
        }

        private static Dictionary<string, Func<T, bool>> _functions;
        public static Dictionary<string, Func<T, bool>> Functions => _functions ?? (_functions = new Dictionary<string, Func<T, bool>>());

        public static string UserFunctionNamespace { get; set; }
    }
}
