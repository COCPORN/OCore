using OCore.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization.Services
{
    public class TenantService : Service, ITenantService
    {
        public Task<Guid> AddAcount(Guid accountId)
        {
            throw new NotImplementedException();
        }

        public Task<Guid> GetTenantAccount(Guid accountId)
        {
            throw new NotImplementedException();
        }
    }
}
