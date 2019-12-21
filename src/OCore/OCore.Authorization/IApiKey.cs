using OCore.Entities;
using OCore.Entities.Data;
using Orleans;
using Orleans.Concurrency;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization
{
    [Immutable]
    public class ApiKeyState
    {
        public Guid Key { get; set; }
        public List<string> Applications { get; set; }
        public string Description { get; set; }
        public bool IsValid { get; set; }
        public string TenantId { get; set; }
        public TimeSpan RateLimitWindow { get; set; }
        public TimeSpan RateLimitOverageDelay { get; set; }
        public long RateInKilobytes { get; set; }
    }

    /// <summary>
    /// Api keys are keyed on guid
    /// </summary>
    /// 
    [DataEntity("OCore.ApiKey")]
    public interface IApiKey : IDataEntity<ApiKeyState>, IGrainWithGuidKey
    {
        Task Activate();
        Task Deactivate();
    }
}
