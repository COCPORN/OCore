using OCore.Authorization.Abstractions;
using OCore.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization
{

    public class AccountInfo
    {
        public string AccountId { get; set; }

        public string TenantId { get; set; }
    }

    [Service("OCore.Token")]
    public interface ITokenService : IService
    {
        [Authorize]
        Task AddToken(Guid token, string account);

        [Authorize]
        Task AddTokenWithTenant(Guid token, string accountId, string tenantId);

        [Authorize(permissions: Permissions.None)]
        Task<AccountInfo> GetAccount(Guid token);
    }
}
