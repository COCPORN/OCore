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
        /// Get the relevant keys based on key strategy. The order of the keys will match the order
        /// of the input
        /// </summary>
        /// <returns></returns>
        protected string[] GetKeys(HttpContext context)
        {
            switch (keyStrategy)
            {
                case KeyStrategy.Identity:
                    return GetIdentityKeys(context);
                case KeyStrategy.Global:
                    return new string[] { "Global" };
                case KeyStrategy.Account:
                    return new string[] { GetAccountId().ToString() };
                case KeyStrategy.AccountPrefix:
                    return GetAccountPrefixedKeys(context);
                case KeyStrategy.Tenant:
                    return new string[] { GetTenantId() };
                case KeyStrategy.TenantPrefix:
                    return GetTenantPrefixedKeys(context);
                case KeyStrategy.AccountCombined:
                    return new string[] { GetAccountCombinedKey(context) };
                case KeyStrategy.AccountCombinedPrefix:
                    return AccountCombinedPrefixedKeys(context);
            }
            return null;
        }

        string[] GetIdentities(string identity)
        {
            return identity.Split(',');
        }

        private string[] AccountCombinedPrefixedKeys(HttpContext context)
        {
            return GetIdentities(GetIdentityFromRoute(context))
                .Select(x => $"{GetAccountCombinedKey(context)}:{x}")
                .ToArray();
        }

        private string[] GetTenantPrefixedKeys(HttpContext context)
        {
            return GetIdentities(GetIdentityFromRoute(context))
                .Select(x => $"{GetTenantId()}:{x}")
                .ToArray();
        }

        private string[] GetAccountPrefixedKeys(HttpContext context)
        {
            return GetIdentities(GetIdentityFromRoute(context))
                .Select(x => $"{GetAccountId()}:{x}")
                .ToArray();
        }

        private string[] GetIdentityKeys(HttpContext context)
        {
            return GetIdentities(GetIdentityFromRoute(context)).ToArray();
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
