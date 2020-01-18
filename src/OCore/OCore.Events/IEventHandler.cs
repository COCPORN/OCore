using Orleans;
using System;
using System.Collections.Generic;
using System.Text;

namespace OCore.Events
{
    public interface IEventHandler : IGrainWithGuidKey
    {
    }
}
