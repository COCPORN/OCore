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

        public EventHandlerAttribute(string eventName) : base(eventName)
        {
            EventName = eventName;
        }

        public EventHandlerAttribute(string eventName, string suffix) : base($"{eventName}:{suffix}")
        {
            EventName = eventName;
            Suffix = suffix;
        }
    }
}
