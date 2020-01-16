using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Events
{
    public interface IEventCounter : IGrainWithGuidKey
    {
        Task<int> Handle();
    }
}
