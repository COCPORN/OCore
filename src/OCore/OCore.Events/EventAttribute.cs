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

        public Func<EventTypeOptions, Task<EventTypeOptions>> OptionsCreator { get; private set; }

        public EventAttribute(string name, EventTypeOptions options = null, Func<EventTypeOptions, Task<EventTypeOptions>> optionsCreaor = null)
        {
            Name = name;
            Options = options;
            OptionsCreator = optionsCreaor;
        }
    }
}
