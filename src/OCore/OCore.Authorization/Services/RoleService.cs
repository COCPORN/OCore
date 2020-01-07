using OCore.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization.Services
{
    public class RoleService : Service, IRoleService
    {
        public Task AddRole(string accountId, string role)
        {
            var accountRolesGrain = GrainFactory.GetGrain<IAccountRoles>(accountId);
            return accountRolesGrain.AddRole(role);
        }

        public Task<List<string>> GetRoles(string accountId)
        {
            var accountRolesGrain = GrainFactory.GetGrain<IAccountRoles>(accountId);
            return accountRolesGrain.GetRoles();
        }

        public Task RemoveRole(string accountId, string role)
        {
            var accountRolesGrain = GrainFactory.GetGrain<IAccountRoles>(accountId);
            return accountRolesGrain.RemoveRole(role);
        }
    }
}
