using System;
using System.Collections.Generic;

namespace Freesia.Internal
{
    class EnvironmentMap : Dictionary<string, Type>
    {
        public EnvironmentMap()
            : base(StringComparer.OrdinalIgnoreCase)
        { }

        public EnvironmentMap(IDictionary<string, Type> source)
            : base(source, StringComparer.OrdinalIgnoreCase)
        { }
    }
}
