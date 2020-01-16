using OCore.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization.Services
{
    public class TokenService : Service, ITokenService
    {
        public async Task AddToken(string token, string account)
        {
            var accountToken = GrainFactory.GetGrain<IAccountToken>(token);
            await accountToken.LinkToAccountId(account);            
        }

        public async Task AddTokenWithTenant(string token, string account, string tenantId)
        {
            var accountToken = GrainFactory.GetGrain<IAccountToken>(token);
            await accountToken.LinkToAccountIdAndTenantId(account, tenantId);
        }

        public Task<AccountInfo> GetAccount(string token)
        {
            var accountToken = GrainFactory.GetGrain<IAccountToken>(token);

            return accountToken.GetAccountId();
        }
    }
}
