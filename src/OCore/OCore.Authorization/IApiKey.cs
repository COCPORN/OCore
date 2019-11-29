using OCore.Entities;
using Orleans;
using Orleans.Concurrency;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization
{
    [Immutable]
    public class ApiKey
    {
        public Guid Key { get; set; }
        public List<string> Applications { get; set; }
        public string Description { get; set; }
        public bool IsValid { get; set; }
        public string TenantId { get; set; }
    }

    /// <summary>
    /// Api keys are keyed on guid
    /// </summary>
    public interface IApiKey : IGrainWithGuidKey, IEntity
    {
        Task Create(string description, string tenantId, List<string> applications);
        Task<ApiKey> GetApiKey();
        Task Activate();
        Task Deactivate();
    }
}
