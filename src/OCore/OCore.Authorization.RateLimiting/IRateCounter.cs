using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
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
        public TimeSpan Window { get; set; }
        public long Limit { get; set; }

        public TimeSpan OverageDelay { get; set; }
    }


    [Serializable]
    [GenerateSerializer]
    public class DataUsage
    {
        public int Bytes { get; set; }
    }
}
