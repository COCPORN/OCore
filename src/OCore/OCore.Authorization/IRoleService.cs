using OCore.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization
{
    [Service("OCore.Role")]
    public interface IRoleService : IService
    {
        [Authorize]
        Task AddRole(string accountId, string role);

        [Authorize]
        Task RemoveRole(string accountId, string role);

        [Authorize]
        Task<List<string>> GetRoles(string accountId);
    }
}
