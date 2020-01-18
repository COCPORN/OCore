using Orleans;
using System;
using System.Collections.Generic;
using System.Text;

namespace OCore.Events
{
    public class EventHandlerAttribute : ImplicitStreamSubscriptionAttribute
    {
        public string EventName { get; private set; }

        public string Suffix { get; private set; }

        public bool ContainExceptions { get; private set; }

        public EventHandlerAttribute(string eventName, bool containExceptions = false) : base(eventName)
        {
            EventName = eventName;
            ContainExceptions = containExceptions;
        }

        public EventHandlerAttribute(string eventName, string suffix, bool containExceptions = false) : base($"{eventName}:{suffix}")
        {
            EventName = eventName;
            Suffix = suffix;
            ContainExceptions = containExceptions;
        }
    }
}
