using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Orleans;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OCore.Entities
{
    [Serializable]
    [GenerateSerializer]
    public class EntityLogic<T> where T : new()
    {
        readonly EntityState<T> state;
        readonly Func<Task> baseWriteState;
        readonly Func<Task> onCreating;
        readonly Func<int, Task<int>> upgrade;
        readonly int version;
        readonly IAddressable addressable;
        readonly ILogger logger;

        JsonSerializerSettings serializerSettings;

        public EntityLogic(EntityState<T> state,
            Func<Task> baseWriteState,
            Func<Task> onCreating,
            Func<int, Task<int>> upgrade,
            IAddressable addressable,
            ILogger logger,
            int version)
        {
            this.state = state;
            this.baseWriteState = baseWriteState;
            this.onCreating = onCreating;
            this.upgrade = upgrade;
            this.addressable = addressable;
            this.logger = logger;
            this.version = version;
        }

        public async Task OnActivateAsync()
        {
            serializerSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
            if (state.Created == false)
            {
                state.CreatedAt = DateTimeOffset.UtcNow;
                state.Data = new T();
                await onCreating();
            }
            else
            {
                if (version != state.Version)
                {
                    await Upgrade();
                }
            }
        }


        public Task WriteStateAsync()
        {
            state.UpdatedAt = DateTimeOffset.UtcNow;
            state.Created = true;
            return baseWriteState();
        }

        public Task Delete()
        {
            List<Task> tasks = new List<Task>();

            state.Created = false;
            state.CreatedAt = DateTimeOffset.MinValue;
            state.UpdatedAt = DateTimeOffset.MinValue;
            state.Data = new T();
            state.Version = 0;
            return baseWriteState();
        }

        private async Task Upgrade()
        {
            // Use the iteration counter to make sure we don't go
            // into an infinite loop
            var iterations = 0;

            // Check against this to see if we should write back
            // migrated state
            var originalVersion = state.Version;

            // Use this for each every iteration to see that each
            // migration step is larger than the previous version
            var previousVersion = state.Version;

            while (version != state.Version)
            {
                var newVersion = await upgrade(state.Version);

                if (newVersion == 0)
                {
                    break;
                }

                if (newVersion <= previousVersion)
                {
                    throw new InvalidOperationException("Migration code didn't increase the version");
                }

                state.Version = newVersion;

                iterations++;

                if (iterations > 100)
                {
                    throw new InvalidOperationException($"Unable to upgrade entity after {iterations} attempts. Have you implemented your upgrade logic correctly?");
                }
                previousVersion = newVersion;
            }

            if (state.Version != originalVersion)
            {
                await WriteStateAsync();
            }
        }
    }
}
