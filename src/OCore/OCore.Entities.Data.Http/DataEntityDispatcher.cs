using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using OCore.Core.Extensions;
using OCore.Authorization.Abstractions.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OCore.Entities.Data.Http
{
    public class GrainKey
    {
        public string Key { get; set; }

        public bool IsFanable { get; set; }
    }

    public abstract class DataEntityDispatcher
    {
        string prefix;
        string dataEntityName;
        KeyStrategy keyStrategy;

        public DataEntityDispatcher(string prefix, string dataEntityName, KeyStrategy keyStrategy)
        {
            this.prefix = prefix ?? string.Empty;
            if (this.prefix.EndsWith("/") == false)
            {
                this.prefix += "/";
            }
            this.dataEntityName = dataEntityName;
            this.keyStrategy = keyStrategy;
        }

        /// <summary>
        /// Get the RoutePattern for the current data entity based on key strategy
        /// </summary>
        /// <returns></returns>
        protected RoutePattern GetRoutePattern(string method = null)
        {
            string methodPostfix = method == null ? "" : $"/{method}";

            switch (keyStrategy)
            {
                case KeyStrategy.Identity:
                    return RoutePatternFactory.Parse($"{prefix}{dataEntityName}/{{identity}}{methodPostfix}");
                case KeyStrategy.Global:
                case KeyStrategy.Account:
                case KeyStrategy.ApiKeyTenant:
                case KeyStrategy.ProjectedAccount:
                case KeyStrategy.ProjectedAccountTenant:
                    return RoutePatternFactory.Parse($"{prefix}{dataEntityName}{methodPostfix}");
                case KeyStrategy.AccountPrefix:
                case KeyStrategy.AccountCombined:
                case KeyStrategy.ApiKeyTenantPrefix:
                case KeyStrategy.ProjectedAccountPrefix:
                case KeyStrategy.ProjectedAccountTenantPrefix:
                case KeyStrategy.ProjectedAccountCombined:
                    return RoutePatternFactory.Parse($"{prefix}{dataEntityName}/{{identity}}{methodPostfix}");
                default:
                    throw new InvalidOperationException("Unknown key strategy");
            }
        }

        /// <summary>
        /// Get the relevant key based on key strategy
        /// </summary>
        /// <returns></returns>
        protected GrainKey GetKey(HttpContext context)
        {
            switch (keyStrategy)
            {
                case KeyStrategy.Identity:
                    return new GrainKey
                    {
                        Key = GetIdentityFromRoute(context),
                        IsFanable = true
                    };
                case KeyStrategy.Global:
                    return new GrainKey
                    {
                        Key = "Global",
                        IsFanable = false
                    };
                case KeyStrategy.Account:
                    return new GrainKey
                    {
                        Key = GetOriginalAccount().ToString(),
                        IsFanable = false
                    };
                case KeyStrategy.AccountPrefix:
                    return new GrainKey
                    {
                        Key = $"{GetOriginalAccount()}:{GetIdentityFromRoute(context)}",
                        IsFanable = true
                    };
                case KeyStrategy.ApiKeyTenant:
                    return new GrainKey
                    {
                        Key = $"{GetTenantIdFromApiKey()}",
                        IsFanable = false
                    };
                case KeyStrategy.ApiKeyTenantPrefix:
                    return new GrainKey
                    {
                        Key = $"{GetTenantIdFromApiKey()}:{GetIdentityFromRoute(context)}",
                        IsFanable = false
                    };
                case KeyStrategy.AccountCombined:
                    return new GrainKey
                    {
                        Key = GetAccountCombinedKey(context),
                        IsFanable = true
                    };
            }
            return null;

        }

        private string GetAccountCombinedKey(HttpContext context)
        {
            var account = GetOriginalAccount();
            var otherId = Guid.Parse(GetIdentityFromRoute(context));
            return account.Combine(otherId).ToString();
        }

        private string GetTenantIdFromApiKey()
        {
            var payload = Payload.Get();
            if (payload.ApiKey != Guid.Empty)
            {
                if (string.IsNullOrEmpty(payload.TenantId) == false)
                {
                    return payload.TenantId;
                } else
                {
                    throw new UnauthorizedAccessException("Tenant ID is not set");
                }
            }
            else
            {
                throw new UnauthorizedAccessException("Payload is missing API key");
            }
        }

        private static string GetIdentityFromRoute(HttpContext context)
        {
            return context.Request.RouteValues["identity"].ToString();
        }

        private Guid GetOriginalAccount()
        {
            var payload = Payload.Get();
            if (payload.AccountIdHasBeenProjected == true)
            {
                return payload.OriginalAccountId.Value;
            }
            else
            {
                if (payload.AccountId == null)
                {
                    throw new UnauthorizedAccessException("There is no account id in the payload");
                }
                else
                {
                    return payload.AccountId.Value;
                }
            }

        }

    }
}
