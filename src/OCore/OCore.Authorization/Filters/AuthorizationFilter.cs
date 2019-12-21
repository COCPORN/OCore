using Microsoft.Extensions.Logging;
using OCore.Authorization.Abstractions.Request;
using Orleans;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization.Filters
{
    public class AuthorizationFilter : IIncomingGrainCallFilter
    {
        IGrainFactory grainFactory;
        readonly ILogger<AuthorizationFilter> logger;

        public AuthorizationFilter(IGrainFactory grainFactory, ILogger<AuthorizationFilter> logger)
        {
            this.grainFactory = grainFactory;
            this.logger = logger;
        }

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            // TODO: Cache this
            var attributes = context.ImplementationMethod.GetCustomAttributes(true);
            var attribute = attributes.FirstOrDefault(x => x is AuthorizeAttribute) as AuthorizeAttribute;

            if (attribute != null && attribute.Requirements != Requirements.None)
            {
                if (RequestContext.Get("I") is Payload payload)
                {
                    // Check to see if request is elevated and elevation is allowed
                    if (attribute.AllowElevatedRequests == false
                        || payload.IsRequestElevated == false)
                    {
                        if (CheckApiKeyRequirement(attribute) || CheckApiKeyAndTokenAndTenantRequirement(attribute))
                        {
                            if (payload.ApiKey != Guid.Empty)
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

                    // Elevate request if it is requested
                    if (attribute.ElevateRequest == true)
                    {
                        payload.IsRequestElevated = true;
                    }
                }
                else
                {
                    throw new UnauthorizedAccessException("Resource requires authorization but context was not supplied with an authorization payload");
                }
            }

            await context.Invoke();
        }

        private async Task CheckTokenAndTenant(IIncomingGrainCallContext context, AuthorizeAttribute attribute, Payload payload)
        {
            await GetIdentity(payload);
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
                    throw new UnauthorizedAccessException("Expected tenant in header");
                }
                var tenantService = grainFactory.GetGrain<ITenantService>(0);

                var tenantAccountId = await tenantService.GetTenantAccount(payload.AccountId.Value);
                
                if (tenantAccountId == Guid.Empty)
                {
                    throw new UnauthorizedAccessException("Invalid tenancy");
                }
                else
                {
                    payload.OriginalAccountId = payload.AccountId;
                    payload.AccountId = tenantAccountId;                    
                    payload.Roles = null;
                }
            }
        }

        private Task CheckRoles(AuthorizeAttribute attribute, IIncomingGrainCallContext context, Payload payload)
        {
            //if (attribute != null)
            //{
            //    //var resourceRole = grainFactory.GetGrain<IResourceAccessDescriptions>(0);
            //    var resourceService = grainFactory.GetGrain<IResourceService>(0);
            //    var resources = await resourceService.GetResources();
            //    var resourceName = attribute.ResourceName;
            //    var requiredPermissions = attribute.Permissions;
            //    var accessDescriptions = await resourceRole.GetAccessDescriptions(resourceName);

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
            return Task.CompletedTask;
        }

        async Task CheckApiKey(string resourceName, Payload payload)
        {
            if (payload.ApiKey == Guid.Empty)
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

        private async Task GetIdentity(Payload payload)
        {
            if (payload.AccountId != null)
            {
                return;
            }

            var tokenService = grainFactory.GetGrain<ITokenService>(0);

            payload.AccountId = await tokenService.GetAccount(payload.Token);
            
            if (!payload.AccountId.HasValue)
            {
                throw new UnauthorizedAccessException("Invalid token");
            }
        }

        async Task GetRolesForAccount(Payload payload)
        {
            if (payload.Roles != null)
            {
                return;
            }

            var roleService = grainFactory.GetGrain<IRoleService>(0);
            payload.Roles = await roleService.GetRoles(payload.AccountId.Value);            
        }


    }
}
