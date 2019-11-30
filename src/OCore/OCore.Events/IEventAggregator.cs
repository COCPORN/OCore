using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Events
{
    public interface IEventAggregator : IGrainWithIntegerKey
    {
        Task Raise<T>(T @event, string streamNameSuffix = null);
    }
}
