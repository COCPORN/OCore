using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OCore.Http
{
    public static class Extensions
    {

        static ConcurrentDictionary<MethodInfo, Func<HttpContext, Task>> asyncFilters 
            = new ConcurrentDictionary<MethodInfo, Func<HttpContext, Task>>();

        public static async Task RunAsyncActionFilters(this HttpContext context, 
            GrainInvoker invoker,
            Func<HttpContext, Task> next)
        {

            if (asyncFilters.TryGetValue(invoker.MethodInfo, out var storedFilters))
            {
                await storedFilters(context);
            }
            else
            {

                var filters = invoker.MethodInfo.GetCustomAttributes(true)
                    .Where(x => x is Filters.IAsyncActionFilter)
                    .Select(x => x as Filters.IAsyncActionFilter)
                    .OrderBy(x => x.Order)
                    .ToArray();

                Func<HttpContext, Task> Wrap(Filters.IAsyncActionFilter filter, Func<HttpContext, Task> n)
                {
                    return new Func<HttpContext, Task>((context) => filter.OnActionExecutionAsync(context, n));
                }

                Func<HttpContext, Task> BuildCallChain(int i, Filters.IAsyncActionFilter filter, Func<HttpContext, Task> n)
                {
                    i--;
                    if (i == 0)
                    {
                        return Wrap(filters[i], n);
                    }
                    return BuildCallChain(i, filters[i], Wrap(filters[i], n));
                }

                int filterCount = filters.Length;
                Func<HttpContext, Task> callChain;
                if (filterCount == 0)
                {
                    callChain = next;
                }
                else
                {
                    callChain = BuildCallChain(filterCount, filters[filterCount - 1], next);
                    asyncFilters.AddOrUpdate(invoker.MethodInfo, callChain, (key, oldValue) => callChain);
                }
                await callChain(context);
            }
        }

        static ConcurrentDictionary<MethodInfo, IEnumerable<IAuthorizationFilter>> authorizationFilters = new ConcurrentDictionary<MethodInfo, IEnumerable<IAuthorizationFilter>>();
        static ConcurrentDictionary<MethodInfo, IEnumerable<ActionFilterAttribute>> actionFilters = new ConcurrentDictionary<MethodInfo, IEnumerable<ActionFilterAttribute>>();

        public static void RunAuthorizationFilters(this HttpContext context, GrainInvoker invoker)
        {
            var actionContext = new ActionContext(context, new RouteData(), new ActionDescriptor());
            var authorizationFilterContext = new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());

            if (authorizationFilters.TryGetValue(invoker.MethodInfo, out var filters))
            {
                RunAuthorizationFilters(authorizationFilterContext, filters);
            }
            else
            {
                var attributes = invoker.MethodInfo.GetCustomAttributes(true)
                    .Concat(invoker.GrainType.GetCustomAttributes(true));
                filters = attributes
                    .Where(x => x is IAuthorizationFilter)
                    .Select(x => x as IAuthorizationFilter);
                authorizationFilters.AddOrUpdate(invoker.MethodInfo, filters, (key, oldvalue) => filters);
                RunAuthorizationFilters(authorizationFilterContext, filters);
            }
        }

        private static void RunAuthorizationFilters(this AuthorizationFilterContext authorizationFilterContext, IEnumerable<IAuthorizationFilter> filters)
        {
            foreach (var filter in filters)
            {
                filter.OnAuthorization(authorizationFilterContext);
            }
        }
    }
}
