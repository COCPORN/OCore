using System;
using System.Collections.Generic;
using System.Text;

namespace OCore.Events
{
    public class PoisonEvent<T>
    {
        public Event<T> Event { get; private set; }

        public PoisonEvent(Event<T> @event)
        {
            Event = @event;
        }
    }
}
