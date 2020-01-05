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
                case KeyStrategy.Global:
                case KeyStrategy.Account:
                case KeyStrategy.Tenant:
                    return RoutePatternFactory.Parse($"{prefix}{dataEntityName}{methodPostfix}");
                case KeyStrategy.Identity:
                case KeyStrategy.AccountPrefix:
                case KeyStrategy.TenantPrefix:
                    return RoutePatternFactory.Parse($"{prefix}{dataEntityName}/{{identity}}{methodPostfix}");
                case KeyStrategy.AccountCombined:
                    return RoutePatternFactory.Parse($"{prefix}{dataEntityName}/{{combination}}{methodPostfix}");
                case KeyStrategy.AccountCombinedPrefix:
                    return RoutePatternFactory.Parse($"{prefix}{dataEntityName}/{{combination}}/{{identity}}{methodPostfix}");
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
                        Key = GetAccountId().ToString(),
                        IsFanable = false
                    };
                case KeyStrategy.AccountPrefix:
                    return new GrainKey
                    {
                        Key = $"{GetAccountId()}:{GetIdentityFromRoute(context)}",
                        IsFanable = true
                    };
                case KeyStrategy.Tenant:
                    return new GrainKey
                    {
                        Key = $"{GetTenantId()}",
                        IsFanable = false
                    };
                case KeyStrategy.TenantPrefix:
                    return new GrainKey
                    {
                        Key = $"{GetTenantId()}:{GetIdentityFromRoute(context)}",
                        IsFanable = false
                    };
                case KeyStrategy.AccountCombined:
                    return new GrainKey
                    {
                        Key = GetAccountCombinedKey(context),
                        IsFanable = true
                    };
                case KeyStrategy.AccountCombinedPrefix:
                    return new GrainKey
                    {
                        Key = $"{GetAccountCombinedKey(context)}:{GetIdentityFromRoute(context)}",
                        IsFanable = true
                    };
            }
            return null;

        }

        private string GetAccountCombinedKey(HttpContext context)
        {
            var account = GetAccountId();
            var otherId = Guid.Parse(GetCombinationFromRoute(context));
            return account.Combine(otherId).ToString();
        }

        private string GetTenantId()
        {
            var payload = Payload.Get();

            if (string.IsNullOrEmpty(payload.TenantId) == false)
            {
                return payload.TenantId;
            }
            else
            {
                throw new UnauthorizedAccessException("Tenant ID is not set");
            }
        }

        private static string GetIdentityFromRoute(HttpContext context)
        {
            return context.Request.RouteValues["identity"].ToString();
        }

        private static string GetCombinationFromRoute(HttpContext context)
        {
            return context.Request.RouteValues["combination"].ToString();
        }

        private Guid GetAccountId()
        {
            var payload = Payload.Get();

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
