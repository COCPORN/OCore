using Orleans;

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
