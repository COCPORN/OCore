using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization
{
    public interface ISystemSetup : IGrainWithIntegerKey
    {
        Task<bool> IsSystemSetup();
        Task SetupSystem();
    }
}
