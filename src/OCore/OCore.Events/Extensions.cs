using Orleans;
using System;
using System.Collections.Generic;
using System.Text;

namespace OCore.Events
{
    public static class Extensions
    {
        public static IEventAggregator GetEventAggregator(this IGrainFactory grainFactory)
        {
            return grainFactory.GetGrain<IEventAggregator>(0);                
        }
    }
}
