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
    public static class RouterExtensions
    {

        public static void RunActionFilters(this HttpContext context, GrainInvoker invoker)
        {
            //var actionExecutingContext = new ActionExecutingContext(new ActionContext(context, new RouteData(), new ActionDescriptor(), new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary()));
            //{

            //};
            //if (actionFilters.TryGetValue(invoker.MethodInfo, out var filters))
            //{
            //    RunActionFilters(authorizationFilterContext, filters);
            //}
            //else
            //{
            //    filters = invoker.MethodInfo.GetCustomAttributes(true)
            //        .Where(x => x is ActionFilterAttribute)
            //        .Select(x => x as ActionFilterAttribute)
            //        .OrderBy(x => x.Order);
            //    actionFilters.Add(invoker.MethodInfo, filters);
            //    RunAuthorizationFilters(authorizationFilterContext, filters);
            //}
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
