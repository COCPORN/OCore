using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization.Services
{
    public class TokenService : Service.Service, ITokenService
    {
        public async Task AddToken(Guid token, Guid account)
        {
            var accountToken = GrainFactory.GetGrain<IAccountToken>(token);
            await accountToken.LinkToAccountId(account);            
        }
    }
}
