using OCore.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization.Services
{
    public class ResourceService : Service, IResourceService
    {
        public Task<List<Resource>> GetResources()
        {
            return Task.FromResult(ResourceEnumerator.Resources);
        }
    }
}
