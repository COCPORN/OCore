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
    [Authorize(Abstractions.Permissions.All, Abstractions.Requirements.TokenAndTenant)]    
    public interface IApiKey : IDataEntity<ApiKeyState>
    {
        Task Activate();
        Task Deactivate();
    }
}
