using OCore.Entities.Data;
using System;
using System.Threading.Tasks;

namespace OCore.Authorization.Grains
{

    public class AccountTokenEntity : DataEntity<AccountToken>, IAccountToken
    {
        public Task<AccountInfo> GetAccountId()
        {
            if (Created == true)
            {
                return Task.FromResult(new AccountInfo
                {
                    AccountId = State.AccountId,
                    TenantId = State.TenantId
                });
            }
            else
            {
                throw new UnauthorizedAccessException("Trying to reach uncreated token");
            }
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
