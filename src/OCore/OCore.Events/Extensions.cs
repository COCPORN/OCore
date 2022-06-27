using Orleans;
using System.Threading.Tasks;

namespace OCore.Events
{
    public static class Extensions
    {
        public static IEventAggregator GetEventAggregator(this IGrainFactory grainFactory)
        {
            return grainFactory.GetGrain<IEventAggregator>(0);
        }

        public static Task RaiseEvent<T>(this IGrainFactory grainFactory, T @event, string streamNameSuffix = null)
        {
            return grainFactory.GetEventAggregator().Raise<T>(@event, streamNameSuffix);
        }
    }
}
