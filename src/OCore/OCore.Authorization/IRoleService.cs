using OCore.Service;
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
        Task AddRole(Guid accountId, string role);

        [Authorize]
        Task RemoveRole(Guid accountId, string role);

        [Authorize]
        Task<List<string>> GetRoles(Guid accountId);
    }
}
