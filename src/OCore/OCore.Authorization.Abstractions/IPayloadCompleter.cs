using OCore.Authorization.Abstractions.Request;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization.Abstractions
{
    public interface IPayloadCompleter
    {
        Task Complete(Payload payload, 
            IClusterClient clusterClient
            );

        Task CheckInitialState(Payload payload,
            IClusterClient clusterClient);

        Task CheckFor(Payload payload,
            IClusterClient clusterClient,
            Permissions permissions,
            Requirements requirements,
            bool allowElevatedRequests = true);
    }
}
