using System;
using System.Collections.Generic;
using System.Text;

namespace OCore.Authorization.Abstractions
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

}
