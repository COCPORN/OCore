using System;
using System.Collections.Generic;
using System.Text;

namespace OCore.Events
{
    public class EventHandlerAttribute : Attribute
    {
        public string EventName { get; private set; }

        public string Suffix { get; private set; }

        public EventHandlerAttribute(string eventName, string suffix = null)
        {
            EventName = eventName;
            Suffix = suffix;
        }
    }
}
