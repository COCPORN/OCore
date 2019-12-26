using OCore.Entities;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization.Grains
{
    public class AccountTokenState
    {
        public Guid AccountId { get; set; }

        public string TenantId { get; set; }

        public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.MaxValue;
    }

    public class AccountTokenGrain : Entity<AccountTokenState>, IAccountToken
    {
        public Task<AccountInfo> GetAccountId()
        {
            return Task.FromResult(new AccountInfo
            {
                AccountId = State.AccountId,
                TenantId = State.TenantId
            });
        }

        public Task LinkToAccountId(Guid accountId)
        {
            State.AccountId = accountId;
            return WriteStateAsync();
        }

        public Task LinkToAccountIdAndTenantId(Guid accountId, string tenantId)
        {
            State.AccountId = accountId;
            State.TenantId = tenantId;
            return WriteStateAsync();
        }
    }
}
