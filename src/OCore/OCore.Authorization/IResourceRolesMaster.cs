using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization
{
    public interface IResourceRolesMaster : IGrainWithIntegerKey
    {
        Task AddResource(string resource);

        Task AddResources(List<string> resources);
        Task SetResources(List<string> resources);

        Task RemoveResource(string resource);
        Task AddAccessDescription(string resource, AccessDescription accessDescription);
        Task RemoveAccessDescription(string resource, AccessDescription accessDescription);

        /// <summary>
        ///   Gets all available resources along with their associated role-permission pair.
        /// </summary>
        Task<Dictionary<string, List<AccessDescription>>> GetMasterData();
    }
}
