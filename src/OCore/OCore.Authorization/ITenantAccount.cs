using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization
{
    /// <summary>
    /// This is keyed on base account ID with the tenant ID as extension
    /// </summary>
    public interface ITenantAccount : IGrainWithGuidCompoundKey
    {
        Task Create(string accountId);

        Task<string> Get();

        Task Delete();
    }
}
