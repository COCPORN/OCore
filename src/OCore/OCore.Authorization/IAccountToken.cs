using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization
{    
    public interface IAccountToken : IGrainWithGuidKey
    {
        Task LinkToAccountId(Guid accountId);

        Task LinkToAccountIdAndTenantId(Guid accountId, string tenantId);
        Task<AccountInfo> GetAccountId();
    }
}
