using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using OCore.Authorization.Abstractions;
using OCore.Authorization.Abstractions.Request;
using Orleans.Runtime;
using System;
using System.Linq;

namespace OCore.Authorization
{
    [AttributeUsage(AttributeTargets.Method
            | AttributeTargets.Class
            | AttributeTargets.Interface,
                       AllowMultiple = false,
                       Inherited = true)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        AuthorizeOptions options;

        public AuthorizeAttribute(Permissions permissions = Permissions.All,
            Requirements requirements = Requirements.ApiKeyOrTokenAndTenant,
            bool allowElevatedRequests = true,
            bool elevateRequest = true)
        {
            Permissions = permissions;
            Requirements = requirements;
            AllowElevatedRequests = allowElevatedRequests;
            ElevateRequest = elevateRequest;

            // Get these from options, somehow
            this.options = new AuthorizeOptions
            {
                ApiKeyHeader = "apikey",
                TenantHeader = "tenant",
                TokenHeader = "token"
            };
        }
        public string ResourceName { get; private set; }

        public Permissions Permissions { get; private set; }

        public Requirements Requirements { get; private set; }

        public bool AllowElevatedRequests { get; private set; }

        public bool ElevateRequest { get; private set; }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var requestPayload = new Payload { };

            // There is now a way to set permissions and requirements
            // on a boundary that is outside the cluster. Therefore we
            // need to carry these forward, and make sure they are checked in
            // the incoming grain call filter when it is first hit, regardless
            // of whether there is an authorization attribute added to the method
            // that is hit.
            requestPayload.InitialPermissions = Permissions;
            requestPayload.InitialRequirements = Requirements;

            if (Guid.TryParse(context.HttpContext.Request.Headers[options.TokenHeader], out var token))
            {
                requestPayload.Token = token;
            }

            if (context.HttpContext.Request.Headers.TryGetValue(options.ApiKeyHeader, out var apiKey))
            {
                requestPayload.ApiKey = apiKey;
            }

            if (string.IsNullOrEmpty(requestPayload.ApiKey) == false && requestPayload.Token != Guid.Empty)
            {
                throw new InvalidOperationException("Both authtoken and apikey provided, please provide one or the other");
            }

            if (context.HttpContext.Request.Headers.TryGetValue(options.TenantHeader, out var t))
            {
                requestPayload.TenantId = t.FirstOrDefault();
            }

            if (requestPayload.ApiKey != null && string.IsNullOrEmpty(requestPayload.TenantId) == false)
            {
                throw new InvalidOperationException("Apikey present with tenant ID, please provide either an apikey or authorizationtoken + tenantId");
            }

            if (Requirements != Requirements.None
                && requestPayload.ApiKey == null
                && requestPayload.Token == Guid.Empty)
            {
                throw new UnauthorizedAccessException("Provide either API key or token");
            }

            RequestContext.Set("I", requestPayload);
        }
    }
}
