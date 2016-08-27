using System;
using System.Collections.Generic;

namespace Freesia.Internal
{
    class EnvironmentMap<T> : Dictionary<string, T>
    {
        public EnvironmentMap()
            : base(StringComparer.OrdinalIgnoreCase)
        { }

        public EnvironmentMap(IDictionary<string, T> source)
            : base(source, StringComparer.OrdinalIgnoreCase)
        { }
    }
}
