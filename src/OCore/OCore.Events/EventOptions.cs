using System;
using System.Collections.Generic;
using System.Text;

namespace OCore.Events
{
    public class EventTypeOptions
    {
        public int WorkerInstances { get; set; }
        public bool Stable { get; set; }
        public bool FireAndForget { get; set; } = true;
        public List<Guid> Destinations { get; set; }        
        public bool HandlePoisonEvents { get; set; }
        public bool ContainedHandling { get; set; }
        public int PoisonLimit { get; set; } = 5;
    }

    public class EventOptions
    {
        public string[] StripFromEventStreamName { get; set; }
        public Dictionary<string, EventTypeOptions> EventTypes { get; set; }
        public string StreamNamePrefix { get; set; }
    }
}
