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
        int maxFanoutLimit;

        public DataEntityDispatcher(string prefix, 
            string dataEntityName, 
            KeyStrategy keyStrategy,
            int maxFanoutLimit)
        {
            this.prefix = prefix ?? string.Empty;
            if (this.prefix.EndsWith("/") == false)
            {
                this.prefix += "/";
            }
            this.dataEntityName = dataEntityName;
            this.keyStrategy = keyStrategy;
            this.maxFanoutLimit = maxFanoutLimit;
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
            string[] keys;
            switch (keyStrategy)
            {
                case KeyStrategy.Identity:
                    keys = GetIdentityKeys(context);
                    break;
                case KeyStrategy.Global:
                    keys = new string[] { "Global" };
                    break;
                case KeyStrategy.Account:
                    keys = new string[] { GetAccountId().ToString() };
                    break;
                case KeyStrategy.AccountPrefix:
                    keys = GetAccountPrefixedKeys(context);
                    break;
                case KeyStrategy.Tenant:
                    keys = new string[] { GetTenantId() };
                    break;
                case KeyStrategy.TenantPrefix:
                    keys = GetTenantPrefixedKeys(context);
                    break;
                case KeyStrategy.AccountCombined:
                    keys = new string[] { GetAccountCombinedKey(context) };
                    break;
                case KeyStrategy.AccountCombinedPrefix:
                    keys = AccountCombinedPrefixedKeys(context);
                    break;
                default:
                    throw new InvalidOperationException("Unknown key strategy");
            }
            if (maxFanoutLimit != 0 && keys.Length > maxFanoutLimit)
            {
                throw new InvalidOperationException("Keys exceed max fanout limit");
            } else
            {
                return keys;
            }
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
            var account = Guid.Parse(GetAccountId());
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

        private string GetAccountId()
        {
            var payload = Payload.Get();

            if (payload.AccountId == null)
            {
                throw new UnauthorizedAccessException("There is no account id in the payload");
            }
            else
            {
                return payload.AccountId;
            }
        }

    }
}
