using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization
{    
    public interface IAccountToken : IGrainWithStringKey
    {
        Task LinkToAccountId(string accountId);

        Task LinkToAccountIdAndTenantId(string accountId, string tenantId);
        Task<AccountInfo> GetAccountId();
    }
}
