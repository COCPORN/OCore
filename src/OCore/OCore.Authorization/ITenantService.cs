using OCore.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization
{
    [Service("OCore.TenantService")]
    public interface ITenantService : IService
    {
        Task<Guid> AddAcount(string accountId);

        Task<string> GetTenantAccount(string accountId);
    }
}
