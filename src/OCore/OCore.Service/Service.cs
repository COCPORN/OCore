using Orleans;
using System;
using System.Collections.Generic;
using System.Text;

namespace OCore.Services
{
    public class Service : Grain
    {
        protected T GetService<T>() where T: IGrainWithIntegerKey
        {
            return GrainFactory.GetGrain<T>(0);
        }

    }
}
