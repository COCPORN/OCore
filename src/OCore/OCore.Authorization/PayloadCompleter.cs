using OCore.Authorization.Abstractions;
using OCore.Authorization.Abstractions.Request;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization
{
    public class PayloadCompleter : IPayloadCompleter
    {
        IClusterClient clusterClient;

        public PayloadCompleter(IClusterClient clusterClient) {
            this.clusterClient = clusterClient;
        }

        /// <summary>
        /// Take a Payload as collected by the [Authorize] attribute and complete
        /// it, as in grab relevant data from relevant sources, etc
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="clusterClient"></param>
        /// <returns></returns>
        public async Task Complete(Payload payload, IClusterClient clusterClient)
        {
            if (payload.IsCompleted == true)
            {
                return;
            }

            await GetIdentity(payload);

            payload.IsCompleted = true;
        }

        private async Task GetIdentity(Payload payload)
        {
            if (payload.AccountId != null)
            {
                return;
            }

            if (payload.Token == Guid.Empty)
            {
                throw new UnauthorizedAccessException("Invalid token");
            }

            var tokenService = clusterClient.GetGrain<ITokenService>(0);
            payload.AccountId = await tokenService.GetAccount(payload.Token);

            if (payload.AccountId.HasValue == false 
                || payload.AccountId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("Invalid token");
            }
        }

    }
}
