using OCore.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization.Grains
{
    public class TenantAccountState
    {
        public Guid TenantAccountId { get; set; }
    }

    public class TenantAccountGrain : Entity<TenantAccountState>, ITenantAccount
    {
        public Task Create(Guid accountId)
        {
            State.TenantAccountId = accountId;
            return WriteStateAsync();
        }

        public Task<Guid> Get()
        {
            return Task.FromResult(State.TenantAccountId);
        }

        Task ITenantAccount.Delete()
        {
            return base.Delete();
        }
    }
}
