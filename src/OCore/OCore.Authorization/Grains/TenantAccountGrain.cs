using OCore.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization.Grains
{
    public class TenantAccountState
    {
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
