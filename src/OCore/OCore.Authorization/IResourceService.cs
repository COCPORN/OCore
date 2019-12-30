using OCore.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization
{
    [Service("OCore.Resource")]
    public interface IResourceService : IService
    {
        [Authorize]
        Task<List<Resource>> GetResources();
        
        Task<List<AccessDescription>> GetAccessDescriptions(string resource);

        Task<IEnumerable<AccountAccessDescription>> GetAccountResources(IEnumerable<string> roles);

    }
}
