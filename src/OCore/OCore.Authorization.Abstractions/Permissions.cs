using System;
using System.Collections.Generic;
using System.Text;

namespace OCore.Authorization.Abstractions
{    
    public enum Requirements
    {
        /// <summary>
        /// This end-point is open, use with caution
        /// </summary>
        None,

        /// <summary>
        /// This end-point needs a token, meaning that a normal login account will suffice
        /// </summary>
        Token,

        /// <summary>        
        /// If an endpoint is open but needs to be open on a per-tenant basis
        /// </summary>
        Tenant,

        /// <summary>        
        /// This end-point needs a <i>projected</i> account, meaning that the accound needs 
        /// to be registered with the relevant tenant
        /// </summary>
        TokenAndTenant,

        /// <summary>
        /// API keys are linked to tenants, so the tenant is implicit
        /// </summary>
        ApiKey, // An API key is always only valid for a tenant,

        /// <summary>
        /// <i>Either</i> supply an API-key <i>or</i> a Token + Tenant
        /// </summary>
        ApiKeyOrTokenAndTenant,
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

}
