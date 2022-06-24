using OCore.Entities;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization.Grains
{
    [Serializable]
    [GenerateSerializer]
    public class AccountRolesState
    {
        public HashSet<string> Roles { get; set; } = new HashSet<string>();
    }

    public class AccountRolesGrain : Entity<AccountRolesState>, IAccountRoles
    {
        public Task AddRole(string role)
        {
            State.Roles.Add(role);
            return WriteStateAsync();
        }

        public Task<List<string>> GetRoles()
        {
            return Task.FromResult(State.Roles.ToList());
        }

        public Task RemoveRole(string role)
        {
            State.Roles.Remove(role);
            return WriteStateAsync();
        }
    }
}
