using Orleans;
using System;
using System.Threading.Tasks;

namespace OCore.Authorization.RateLimiting
{
    public interface IRateCounter
    {
        Task<bool> CheckRateLimit(ApiKeySettings settings);

        Task ReportDataUsage(DataUsage dataUsage);
    }


    [Serializable]
    [GenerateSerializer]
    public class ApiKeySettings
    {
        [Id(0)]
        public TimeSpan Window { get; set; }
        [Id(1)]
        public long Limit { get; set; }

        [Id(2)]
        public TimeSpan OverageDelay { get; set; }
    }


    [Serializable]
    [GenerateSerializer]
    public class DataUsage
    {
        [Id(0)]
        public int Bytes { get; set; }
    }
}
