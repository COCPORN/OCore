using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Events
{
    public class EventAttribute : Attribute
    {
        public string Name { get; private set; }

        public EventTypeOptions Options { get; private set; }

        public EventAttribute(string name, 
            bool fireAndForget = true,             
            int poisonLimit = 0,
            string providerName = null,
            bool stable = true,
            bool trackAndKillPoisonEvents = false,
            int workerInstances = 4)
        {
            Name = name;
            Options = new EventTypeOptions
            {
                FireAndForget = fireAndForget,                
                PoisonLimit = poisonLimit,
                ProviderName = providerName,
                Stable = stable,
                TrackAndKillPoisonEvents = trackAndKillPoisonEvents,
                WorkerInstances = workerInstances
            };
        }
    }
}
