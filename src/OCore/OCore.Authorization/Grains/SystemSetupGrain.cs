using OCore.Entities;
using Orleans;
using System;
using System.Threading.Tasks;

namespace OCore.Authorization.Grains
{
    [Serializable]
    [GenerateSerializer]
    public class SystemSetupState
    {
        [Id(0)]
        public bool SystemSetup { get; set; }

        [Id(1)]
        public DateTimeOffset SetupTime { get; set; }
    }

    public class SystemSetupGrain : Entity<SystemSetupState>, ISystemSetup
    {
        public Task<bool> IsSystemSetup()
        {
            return Task.FromResult(State.SystemSetup);
        }

        public async Task SetupSystem()
        {
            State.SystemSetup = true;
            State.SetupTime = DateTimeOffset.UtcNow;
            await WriteStateAsync();
        }
    }
}
