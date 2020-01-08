using Orleans;
using System;
using System.Collections.Generic;
using System.Text;

namespace OCore.Services
{
    public static class Extensions
    {
        public static T GetService<T>(this IGrainFactory grainFactory) where T : IGrainWithIntegerKey
        {
            return grainFactory.GetGrain<T>(0);
        }
    }
}
