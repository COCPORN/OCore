using OCore.Entities;
using Orleans;
using System;
using System.Threading.Tasks;

namespace OCore.Authorization.Grains
{
    [Serializable]
    [GenerateSerializer]
    public class TenantAccountState
    {
        [Id(0)]
        public string TenantAccountId { get; set; }
    }

    public class TenantAccountGrain : Entity<TenantAccountState>, ITenantAccount
    {
        public Task Create(string accountId)
        {
            State.TenantAccountId = accountId;
            return WriteStateAsync();
        }

        public Task<string> Get()
        {
            return Task.FromResult(State.TenantAccountId);
        }

        Task ITenantAccount.Delete()
        {
            return base.Delete();
        }
    }
}
