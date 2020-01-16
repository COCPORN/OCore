using OCore.Entities;
using OCore.Entities.Data;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization.Grains
{

    public class AccountTokenEntity : DataEntity<AccountToken>, IAccountToken
    {
        public Task<AccountInfo> GetAccountId()
        {
            return Task.FromResult(new AccountInfo
            {
                AccountId = State.AccountId,
                TenantId = State.TenantId
            });
        }

        public Task LinkToAccountId(string accountId)
        {
            State.AccountId = accountId;
            return WriteStateAsync();
        }

        public Task LinkToAccountIdAndTenantId(string accountId, string tenantId)
        {
            State.AccountId = accountId;
            State.TenantId = tenantId;
            return WriteStateAsync();
        }
    }
}
