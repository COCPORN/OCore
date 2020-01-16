using OCore.Core.Extensions;
using OCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization.Services
{
    public class ResourceService : Service, IResourceService
    {
        Dictionary<string, List<AccessDescription>> resourceRoles { get; set; } = new Dictionary<string, List<AccessDescription>>();

        public Task<List<Resource>> GetResources()
        {
            return Task.FromResult(ResourceEnumerator.Resources);
        }

        public void SetResourceRoles(Dictionary<string, List<AccessDescription>> resourceRoles)
        {
            this.resourceRoles = resourceRoles;
        }

        public Task<List<AccessDescription>> GetAccessDescriptions(string resource)
        {
            var accessDescriptions = new Dictionary<string, AccessDescription>();
            do
            {
                if (resourceRoles.TryGetValue(resource, out var ads))
                {
                    foreach (var ad in ads)
                    {
                        if (accessDescriptions.ContainsKey(ad.Role) == false)
                        {
                            accessDescriptions.Add(ad.Role, ad);
                        }
                    }
                }
            } while (resource.TryChomp(out resource));

            if (resourceRoles.TryGetValue(String.Empty, out var rootDescriptors))
            {
                foreach (var ad in rootDescriptors)
                {
                    if (accessDescriptions.ContainsKey(ad.Role) == false)
                    {
                        accessDescriptions.Add(ad.Role, ad);
                    }
                }
            }

            return Task.FromResult(accessDescriptions.Values.ToList());
        }

        public Task<IEnumerable<AccountAccessDescription>> GetAccountResources(IEnumerable<string> roles)
        {
            //return Task.FromResult(accessDescriptions.GetAccountAccessByRoles(roles));
            return null;
        }
    }
}
