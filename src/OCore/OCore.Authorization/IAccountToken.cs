using OCore.Entities.Data;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization
{
    public class AccountToken
    {
        public string AccountId { get; set; }

        public string TenantId { get; set; }

        public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.MaxValue;
    }

    public interface IAccountToken : IDataEntity<AccountToken>
    {
        Task LinkToAccountId(string accountId);

        Task LinkToAccountIdAndTenantId(string accountId, string tenantId);
        Task<AccountInfo> GetAccountId();
    }
}
