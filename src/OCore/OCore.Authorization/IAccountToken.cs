using OCore.Entities.Data;
using Orleans;
using System;
using System.Threading.Tasks;

namespace OCore.Authorization
{
    [Serializable]
    [GenerateSerializer]
    public class AccountToken
    {
        [Id(0)]
        public string AccountId { get; set; }

        [Id(1)]
        public string TenantId { get; set; }

        [Id(2)]
        public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.MaxValue;
    }

    public interface IAccountToken : IDataEntity<AccountToken>
    {
        Task LinkToAccountId(string accountId);
        Task LinkToAccountIdAndTenantId(string accountId, string tenantId = "Default");
        Task<AccountInfo> GetAccountId();
    }
}
