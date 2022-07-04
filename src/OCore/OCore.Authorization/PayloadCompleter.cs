using OCore.Authorization.Abstractions;
using OCore.Authorization.Abstractions.Request;
using OCore.Entities.Data;
using OCore.Entities.Data.Extensions;
using Orleans;
using System;
using System.Threading.Tasks;

namespace OCore.Authorization
{
    /// <summary>
    /// This is an implementation of the IPayloadCompleter interface
    /// that works with the internal authorization platform for OCore
    /// </summary>
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

            // Given a token in the payload, get account id and roles, etc
            if (payload.Token != null)
            {
                await GetIdentity(payload);

                if (string.IsNullOrEmpty(payload.TenantId) == false
                    && payload.AccountIdHasBeenProjected == false)
                {
                    await GetProjectedIdentity(payload);
                }

                await GetRolesForAccount(payload);
            }
            else
            {
                await GetApiKeyApplications(payload);
            }

            payload.IsCompleted = true;
        }


        async Task GetApiKeyApplications(Payload payload)
        {
            if (payload.ApiKey == null)
            {
                throw new UnauthorizedAccessException("API key required");
            }
            if (payload.ApiKeyApplications == null)
            {
                var apiKeyCache = clusterClient.GetGrain<IApiKeyCache>(payload.ApiKey);
                var apiKey = await apiKeyCache.GetApiKey();
                if (apiKey == null || apiKey.IsValid == false)
                {
                    throw new UnauthorizedAccessException("Invalid API key");
                }
                payload.ApiKeyApplications = apiKey.Applications;
                payload.TenantId = apiKey.TenantId;
            }
        }

        async Task GetRolesForAccount(Payload payload)
        {
            if (payload.Roles != null)
            {
                return;
            }

            var roleService = clusterClient.GetGrain<IRoleService>(0);
            payload.Roles = await roleService.GetRoles(payload.AccountId);
        }

        async Task GetProjectedIdentity(Payload payload)
        {
            var tenantAccountGrain = clusterClient.GetGrain<ITenantAccount>($"{payload.Token}:{payload.TenantId}");
            var projectedAccountId = await tenantAccountGrain.Get();
            payload.AccountIdHasBeenProjected = true;
            payload.OriginalAccountId = payload.AccountId;
            payload.ProjectedAccountId = projectedAccountId;
        }

        private async Task GetIdentity(Payload payload)
        {
            if (payload.AccountId != null)
            {
                return;
            }

            if (payload.Token == null)
            {
                throw new UnauthorizedAccessException("Invalid token");
            }

            var tokenGrain = clusterClient.GetDataEntity<IAccountToken>(payload.Token);
            try
            {
                var accountInfo = await tokenGrain.Read();

                if (string.IsNullOrEmpty(accountInfo.TenantId) == false)
                {
                    payload.ProjectedAccountId = accountInfo.AccountId;
                    payload.TenantId = accountInfo.TenantId;
                    payload.AccountIdHasBeenProjected = true;
                }
                else
                {
                    payload.OriginalAccountId = accountInfo.AccountId;
                }

                if (payload.AccountId == null)
                {
                    throw new UnauthorizedAccessException("Invalid token");
                }
            }
            catch (DataCreationException ex)
            {
                throw new UnauthorizedAccessException("Invalid token", ex);
            }
        }

    }
}
