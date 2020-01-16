using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Entities
{
    public interface IEntity : IGrain
    {
        Task ReadStateAsync();
    }
}
