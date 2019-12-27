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

        public PayloadCompleter(IClusterClient clusterClient)
        {
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

                if (string.IsNullOrEmpty(payload.TenantId) == false 
                    && payload.AccountIdHasBeenProjected == false)
                {
                    await GetProjectedIdentity(payload);
                }
            }

            payload.IsCompleted = true;
        }

        private async Task GetProjectedIdentity(Payload payload)
        {
            var tenantAccountGrain = clusterClient.GetGrain<ITenantAccount>(payload.Token, payload.TenantId);
            var projectedAccountId = await tenantAccountGrain.Get();
            payload.AccountIdHasBeenProjected = true;
            payload.OriginalAccountId = payload.AccountId;
            payload.AccountId = projectedAccountId;
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
            
            if (string.IsNullOrEmpty(accountInfo.TenantId) == false)
            {
                payload.TenantId = accountInfo.TenantId;
                payload.AccountIdHasBeenProjected = true;
            }

            if (payload.AccountId.HasValue == false
                || payload.AccountId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("Invalid token");
            }
        }
   
    }
}
