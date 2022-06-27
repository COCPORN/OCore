using OCore.Entities.Data;
using Orleans;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OCore.Authorization
{
    [Immutable]
    [GenerateSerializer]
    public class ApiKeyState
    {
        [Id(0)]
        public List<string> Applications { get; set; }
        [Id(1)]
        public string Description { get; set; }
        [Id(2)]
        public bool IsValid { get; set; }
        [Id(3)]
        public string TenantId { get; set; }
        [Id(4)]
        public TimeSpan RateLimitWindow { get; set; }
        [Id(5)]
        public TimeSpan RateLimitOverageDelay { get; set; }
        [Id(6)]
        public long RateInKilobytes { get; set; }
    }

    /// <summary>
    /// Api keys are keyed on guid
    /// </summary>
    /// 
    [Authorize(Abstractions.Permissions.All, Abstractions.Requirements.TokenAndTenant)]
    public interface IApiKey : IDataEntity<ApiKeyState>
    {
        Task Activate();
        Task Deactivate();
    }
}
