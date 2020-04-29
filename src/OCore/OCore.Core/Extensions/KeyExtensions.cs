using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace OCore.Core.Extensions
{
    public static class Extensions
    {
        public static Key Key(this IAddressable grain)
        {
            return Core.Key.FromGrain(grain);
        }
    }
}
