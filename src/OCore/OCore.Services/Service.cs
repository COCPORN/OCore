using Orleans;
using Orleans.Concurrency;
using System;
using System.Collections.Generic;
using System.Text;

namespace OCore.Services
{
    [StatelessWorker]
    [Reentrant]
    public class Service : Grain
    {
        protected T GetService<T>() where T: IGrainWithIntegerKey
        {
            return GrainFactory.GetGrain<T>(0);
        }

    }
}
