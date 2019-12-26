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
        public async Task Complete(Payload payload, 
            IClusterClient clusterClient)
        {
            // If the payload is completed, this has run to completion previously
            if (payload.IsCompleted == true)
            {
                return;
            }

            if (payload.Token != Guid.Empty)
            {
                await GetIdentity(payload);

                if (string.IsNullOrEmpty(payload.TenantId) == false)
                {
                    await GetProjectedIdentity(payload);
                }
            }

            payload.IsCompleted = true;
        }

        private Task GetProjectedIdentity(Payload payload)
        {
            throw new NotImplementedException();
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
            var accountInfo = await tokenService.GetAccount(payload.Token);

            payload.AccountId = accountInfo.AccountId;
            payload.TenantId = accountInfo.TenantId;

            if (payload.AccountId.HasValue == false 
                || payload.AccountId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("Invalid token");
            }
        }

        public async Task CheckInitialState(Payload payload, IClusterClient clusterClient)
        {
            if (payload.IsInitialStateSatisfied == false)
            {
                switch (payload.InitialRequirements)
                {
                    case Requirements.Token:
                        await GetIdentity(payload);
                        break;
                    case Requirements.TokenAndTenant:
                        await GetProjectedIdentity(payload);
                        break;
                }
                payload.IsInitialStateSatisfied = true;
            }

            payload.IsInitialStateSatisfied = true;
        }

        public Task CheckFor(Payload payload, IClusterClient clusterClient, Permissions permissions, Requirements requirements, bool allowElevatedRequests = true)
        {
            throw new NotImplementedException();
        }
    }
}
