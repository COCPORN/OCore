using OCore.Entities;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OCore.Authorization.Grains
{
    [Serializable]
    [GenerateSerializer]
    public class ResourceRolesMasterGrainState
    {
        [Id(0)]
        public Dictionary<string, List<AccessDescription>> AccessDescriptions { get; set; } = new Dictionary<string, List<AccessDescription>>();
    }

    public class ResourceRolesMasterGrain : Entity<ResourceRolesMasterGrainState>, IResourceRolesMaster
    {

        bool IsNewResourceValid(string resource)
        {
            if (resource == "")
            {
                return true;
            }

            //return GrainInterfaceInfo.Resources.Any(x => ResourceHelpers.IsSuperResourceOf(resource, x.ResourceName));
            return true;
        }

        public Task AddResource(string resource)
        {
            if (State.AccessDescriptions.ContainsKey(resource))
            {
                return Task.CompletedTask;
            }
            else
            {
                if (IsNewResourceValid(resource))
                {
                    State.AccessDescriptions.Add(resource, new List<AccessDescription>());
                }
                else
                {
                    throw new InvalidOperationException("Attempted to add an invalid resource");
                }
                return WriteStateAsync();
            }

        }

        public Task AddAccessDescription(string resource, AccessDescription accessDescription)
        {
            if (!State.AccessDescriptions.ContainsKey(resource))
            {
                if (IsNewResourceValid(resource))
                {
                    State.AccessDescriptions.Add(resource, new List<AccessDescription>());
                }
                else
                {
                    throw new InvalidOperationException("Attempted to add an invalid resource");
                }
            }

            var accessDescriptions = State.AccessDescriptions[resource];
            if (accessDescriptions.Contains(accessDescription))
            {
                return Task.CompletedTask;
            }
            else
            {
                accessDescriptions.Add(accessDescription);
                return WriteStateAsync();
            }
        }

        public Task<Dictionary<string, List<AccessDescription>>> GetMasterData()
        {
            return Task.FromResult(State.AccessDescriptions);
        }

        public Task RemoveResource(string resource)
        {
            if (State.AccessDescriptions.ContainsKey(resource))
            {
                State.AccessDescriptions.Remove(resource);
                return WriteStateAsync();
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        public Task RemoveAccessDescription(string resource, AccessDescription accessDescription)
        {
            if (State.AccessDescriptions.ContainsKey(resource))
            {
                var accessDescriptions = State.AccessDescriptions[resource];
                if (accessDescriptions.Remove(accessDescription))
                {
                    return WriteStateAsync();
                }
                else
                {
                    return Task.CompletedTask;
                }
            }
            else
            {
                throw new InvalidOperationException("Unknown resource");
            }
        }


        public Task SetResources(List<string> resources)
        {
            var validResources = resources.Where(r => IsNewResourceValid(r));
            var protectedResources = new HashSet<string>();

            resources.Add("");

            foreach (var resource in resources)
            {
                var allResources = ResourceHelpers.GetAllResources(resource);
                foreach (var subResource in allResources)
                {
                    if (protectedResources.Contains(subResource) == false)
                    {
                        protectedResources.Add(subResource);
                    }
                }
                if (State.AccessDescriptions.ContainsKey(resource))
                {
                    continue;
                }
                else
                {
                    if (IsNewResourceValid(resource))
                    {
                        State.AccessDescriptions.Add(resource, new List<AccessDescription>());
                    }
                }
            }

            var deleteAccessDescriptions = State
                .AccessDescriptions
                .Keys
                .Where(accessDescription => protectedResources.Contains(accessDescription) == false)
                .ToList();

            foreach (var deleteKey in deleteAccessDescriptions)
            {
                State.AccessDescriptions.Remove(deleteKey);
            }

            return WriteStateAsync();

        }

        public Task AddResources(List<string> resources)
        {
            bool writeState = false;
            foreach (var resource in resources)
            {
                if (State.AccessDescriptions.ContainsKey(resource))
                {
                    continue;
                }
                else
                {
                    if (IsNewResourceValid(resource))
                    {
                        State.AccessDescriptions.Add(resource, new List<AccessDescription>());
                        writeState = true;
                    }
                }
            }
            return writeState ? WriteStateAsync() : Task.CompletedTask;
        }
    }
}
