using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization
{
    public interface IAccountRoles : IGrainWithStringKey
    {
        Task AddRole(string role);

        Task RemoveRole(string role);

        Task<List<string>> GetRoles();
    }
}
