using Orleans;
using System;
using System.Collections.Generic;
using System.Text;

namespace OCore.Entities.Data.Extensions
{
    public static class Extensions
    {
        public static T GetDataEntity<T>(this IGrainFactory grainFactory, string key) where T : IDataEntity
        {
            return grainFactory.GetGrain<T>(key);
        }

        public static T GetDataEntity<T>(this IGrainFactory grainFactory, string prefix, string identity) where T : IDataEntity
        {
            return grainFactory.GetGrain<T>($"{prefix}:{identity}");
        }
    }
}
