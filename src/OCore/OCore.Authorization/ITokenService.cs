using OCore.Authorization.Abstractions;
using OCore.Services;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization
{

    [Serializable]
    [GenerateSerializer]
    public class AccountInfo
    {
        public string AccountId { get; set; }

        public string TenantId { get; set; }
    }

    [Service("OCore.Token")]
    public interface ITokenService : IService
    {
        [Authorize]
        Task AddToken(string token, string account);

        [Authorize]
        Task AddTokenWithTenant(string token, string accountId, string tenantId);

        [Authorize(permissions: Permissions.None)]
        Task<AccountInfo> GetAccount(string token);
    }
}
