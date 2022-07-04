using Microsoft.Extensions.Logging;
using OCore.Authorization.Abstractions;
using OCore.Authorization.Abstractions.Request;
using Orleans;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OCore.Authorization.Filters
{
    public class AuthorizationFilter : IIncomingGrainCallFilter
    {
        IGrainFactory grainFactory;
        IClusterClient clusterClient;
        readonly ILogger<AuthorizationFilter> logger;
        IPayloadCompleter payloadCompleter;

        public AuthorizationFilter(IGrainFactory grainFactory,
            IClusterClient clusterClient,
            IPayloadCompleter payloadCompleter,
            ILogger<AuthorizationFilter> logger)
        {
            this.grainFactory = grainFactory;
            this.clusterClient = clusterClient;
            this.payloadCompleter = payloadCompleter;
            this.logger = logger;
        }

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            // I am not 100% sure why this happens. The null check seems to be
            // necessary on calls that come from a timer.
            if (context?.ImplementationMethod == null)
            {
                await context.Invoke();
                return;
            }

            var attributes = context.ImplementationMethod.GetCustomAttributes(true);
            var attribute = attributes.FirstOrDefault(x => x is AuthorizeAttribute) as AuthorizeAttribute;

            if (attribute == null || attribute.Requirements == Requirements.None)
            {
                await context.Invoke();
                return;
            }

            var payload = Payload.Get();

            await payloadCompleter.Complete(payload, clusterClient);


            if (attribute != null && attribute.Requirements != Requirements.None)
            {
                // Check to see if request is elevated and elevation is allowed
                if (attribute.AllowElevatedRequests == false
                    || payload.IsRequestElevated == false)
                {
                    if (CheckApiKeyRequirement(attribute) || CheckApiKeyAndTokenAndTenantRequirement(attribute))
                    {
                        if (string.IsNullOrEmpty(payload.ApiKey) == false)
                        {
                            await CheckApiKey(attribute.ResourceName, payload);
                        }
                        else
                        {
                            await CheckTokenAndTenant(context, attribute, payload);
                        }
                    }
                    else
                    {
                        await CheckTokenAndTenant(context, attribute, payload);
                    }
                }

                if (attribute.ElevateRequest == true)
                {
                    payload.IsRequestElevated = true;
                }
            }
            else
            {
                throw new UnauthorizedAccessException("Resource requires authorization but context was not supplied with an authorization payload");
            }

            await context.Invoke();
        }

        private async Task CheckTokenAndTenant(IIncomingGrainCallContext context, AuthorizeAttribute attribute, Payload payload)
        {
            await CheckTenancy(attribute, payload);
            await GetRolesForAccount(payload);
            await CheckRoles(attribute, context, payload);
        }

        private bool CheckApiKeyAndTokenAndTenantRequirement(AuthorizeAttribute attribute)
        {
            return (attribute.Requirements & Requirements.ApiKeyOrTokenAndTenant) == Requirements.ApiKeyOrTokenAndTenant;
        }

        private static bool CheckApiKeyRequirement(AuthorizeAttribute attribute)
        {
            return (attribute.Requirements & Requirements.ApiKey) == Requirements.ApiKey;
        }


        async Task CheckTenancy(AuthorizeAttribute attribute, Payload payload)
        {
            // If the payload already has a projected account ID, 
            // the tenant is also present and validated
            if (payload.AccountIdHasBeenProjected == true)
            {
                return;
            }

            if ((attribute.Requirements & Requirements.Tenant) == Requirements.Tenant)
            {
                if (payload.TenantId == null)
                {
                    throw new UnauthorizedAccessException("Expected tenant in header or in login payload");
                }
                var tenantService = grainFactory.GetGrain<ITenantService>(0);

                var tenantAccountId = await tenantService.GetTenantAccount(payload.AccountId);

                if (tenantAccountId == null)
                {
                    throw new UnauthorizedAccessException("Invalid tenancy");
                }
                else
                {
                    payload.OriginalAccountId = payload.AccountId;
                    payload.ProjectedAccountId = tenantAccountId;
                    payload.Roles = null;
                }
            }
        }

        Task CheckRoles(AuthorizeAttribute attribute, IIncomingGrainCallContext context, Payload payload)
        {
            return null;
            //if (attribute != null)
            //{                
            //    var resourceService = grainFactory.GetGrain<IResourceService>(0);
            //    var resources = await resourceService.GetResources();
            //    var resourceName = attribute.ResourceName;
            //    var requiredPermissions = attribute.Permissions;
            //    var accessDescriptions = await resourceService.GetAccessDescriptions(resourceName);

            //    if (accessDescriptions.Count == 0)
            //    {
            //        throw new UnauthorizedAccessException("Resource is unreachable");
            //    }

            //    foreach (var role in payload.Roles)
            //    {
            //        if (accessDescriptions.Any(x => x.Role == role && x.Permissions.HasFlag(attribute.Permissions)))
            //        {
            //            return;
            //        }
            //    }
            //    throw new UnauthorizedAccessException();
            //}
            //return Task.CompletedTask;
        }

        async Task CheckApiKey(string resourceName, Payload payload)
        {
            if (payload.ApiKey == null)
            {
                throw new UnauthorizedAccessException("API key required");
            }
            if (payload.ApiKeyApplications == null)
            {
                var apiKeyCache = grainFactory.GetGrain<IApiKeyCache>(payload.ApiKey);
                var apiKey = await apiKeyCache.GetApiKey();
                if (apiKey == null || apiKey.IsValid == false)
                {
                    throw new UnauthorizedAccessException("Invalid API key");
                }
                payload.ApiKeyApplications = apiKey.Applications;
                payload.TenantId = apiKey.TenantId;
            }
            bool authorizationOk = false;
            foreach (var application in payload.ApiKeyApplications)
            {
                if (resourceName.StartsWith(application))
                {
                    authorizationOk = true;
                    break;
                }
            }
            if (authorizationOk == false)
            {
                throw new UnauthorizedAccessException("API key is not valid for this application");
            }
        }

        async Task GetRolesForAccount(Payload payload)
        {
            if (payload.Roles != null)
            {
                return;
            }

            var roleService = grainFactory.GetGrain<IRoleService>(0);
            payload.Roles = await roleService.GetRoles(payload.AccountId);
        }


    }
}
