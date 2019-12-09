using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using OCore.Authorization.Abstractions.Request;
using Orleans.Runtime;
using System;
using System.Linq;

namespace OCore.Authorization
{
    [Flags]
    public enum Requirements
    {
        /// <summary>
        /// This end-point is open, use with caution
        /// </summary>
        None = 0,

        /// <summary>
        /// This end-point needs a token, meaning that a normal login account will suffice
        /// </summary>
        Token = 1,

        /// <summary>
        /// TODO: Where do we use this?
        /// I am not sure when we only use Tenant 
        /// </summary>
        Tenant = 2,

        /// <summary>        
        /// This end-point needs a <i>projected</i> account, meaning that the accound needs 
        /// to be registered with the relevant tenant
        /// </summary>
        TokenAndTenant = Token | Tenant,

        /// <summary>
        /// API keys are linked to tenants, so the tenant is implicit
        /// </summary>
        ApiKey = 4, // An API key is always only valid for a tenant,

        /// <summary>
        /// <i>Either</i> supply an API-key <i>or</i> a Token + Tenant
        /// </summary>
        ApiKeyOrTokenAndTenant = 8,
    }

    [Flags]
    public enum Permissions
    {
        None = 0,
        Read = 1,
        Write = 2,
        ReadWrite = Read | Write,
        All = ReadWrite
    }


    [AttributeUsage(AttributeTargets.Method,
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

            if (Guid.TryParse(context.HttpContext.Request.Headers[options.TokenHeader], out var token))
            {
                requestPayload.Token = token;
            }

            if (Guid.TryParse(context.HttpContext.Request.Headers[options.ApiKeyHeader], out var apiKey))
            {
                requestPayload.ApiKey = apiKey;
            }

            if (requestPayload.ApiKey != Guid.Empty && requestPayload.Token != Guid.Empty)
            {
                throw new InvalidOperationException("Both authtoken and apikey provided, please provide one or the other");
            }

            if (context.HttpContext.Request.Headers.TryGetValue(options.TokenHeader, out var t))
            {
                requestPayload.TenantId = t.FirstOrDefault();
            }

            if (requestPayload.ApiKey != Guid.Empty && string.IsNullOrEmpty(requestPayload.TenantId) == false)
            {
                throw new InvalidOperationException("Apikey present with tenant ID, please provide either an apikey or authorizationtoken + tenantId");
            }

            if (Requirements != Requirements.None 
                && requestPayload.ApiKey == Guid.Empty 
                && requestPayload.Token == Guid.Empty)
            {
                throw new UnauthorizedAccessException("Provide either API key or token");
            }

            RequestContext.Set("I", requestPayload);
        }
    }
}
