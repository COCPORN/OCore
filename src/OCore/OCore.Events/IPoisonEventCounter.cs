using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Events
{
    /// <summary>
    /// The event counter is keyed on Guid that matches the message ID.
    /// 
    /// The implementation is a grain without persisted state, reasoning that it
    /// will probably fail within a reasonable time window, and the consequence
    /// of it failing 10 times instead of 5 before being marked as poisonous
    /// is probably negligable.
    /// 
    /// Also, it will probably fail in deterministic order, so just tracking the
    /// message id is probably sufficient for poison marking.
    /// </summary>
    public interface IPoisonEventCounter : IGrainWithGuidKey
    {
        Task<int> Handle();
    }
}
