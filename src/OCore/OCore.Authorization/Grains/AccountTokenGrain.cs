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

        public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.MaxValue;
    }

    public class AccountTokenGrain : Grain<AccountTokenState>, IAccountToken
    {
        public Task<Guid> GetAccountId()
        {
            return Task.FromResult(State.AccountId);
        }

        public Task LinkToAccountId(Guid accountId)
        {
            State.AccountId = accountId;
            return WriteStateAsync();
        }
    }
}
