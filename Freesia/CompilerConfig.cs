using System;
using System.Collections.Generic;

namespace Freesia
{
    public class CompilerConfig<T>
    {
        protected class UserFunctionTypePlaceholder { }

        private static Dictionary<string, Func<T, bool>> _functions;
        public static Dictionary<string, Func<T, bool>> Functions => _functions ?? (_functions = new Dictionary<string, Func<T, bool>>());

        public static string UserFunctionNamespace { get; set; }
    }
}
