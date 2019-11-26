using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OCore.Service.Http
{
    public class ServiceRouter
    {
        IClusterClient clusterClient;
        IServiceProvider serviceProvider;
        ILogger logger;

        public ServiceRouter(IClusterClient clusterClient,
            IServiceProvider serviceProvider,
            ILogger<ServiceRouter> logger)
        {
            this.clusterClient = clusterClient;
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        private readonly Dictionary<string, GrainInvoker> routes = new Dictionary<string, GrainInvoker>(StringComparer.InvariantCultureIgnoreCase);

        public void RegisterRoute(string pattern, MethodInfo methodInfo)
        {
            CheckGrainType(methodInfo.DeclaringType);
            routes.Add(pattern, new GrainInvoker(serviceProvider, methodInfo));
        }

        private void CheckGrainType(Type grainInterfaceType)
        {
            var ifaces = grainInterfaceType.GetInterfaces();
            if (ifaces.Contains(typeof(IGrainWithIntegerKey)) == false)
            {
                throw new InvalidOperationException("Service is not of correct type");
            }

        }

        public Task Dispatch(HttpContext context)
        {
            AddCors(context);
            var endpoint = (RouteEndpoint)context.GetEndpoint();
            var pattern = endpoint.RoutePattern;

            var invoker = routes[pattern.RawText];
            RunAuthorizationFilters(context, invoker);
            RunActionFilters(context, invoker);

            var grain = clusterClient.GetGrain(invoker.GrainType, 0);
            if (grain == null)
            {
                context.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                return Task.CompletedTask;
            }

            return invoker.Invoke(grain, context);
        }

        private void RunActionFilters(HttpContext context, GrainInvoker invoker)
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

        Dictionary<MethodInfo, IEnumerable<IAuthorizationFilter>> authorizationFilters = new Dictionary<MethodInfo, IEnumerable<IAuthorizationFilter>>();
        Dictionary<MethodInfo, IEnumerable<ActionFilterAttribute>> actionFilters = new Dictionary<MethodInfo, IEnumerable<ActionFilterAttribute>>();

        private void RunAuthorizationFilters(HttpContext context, GrainInvoker invoker)
        {
            var actionContext = new ActionContext(context, new RouteData(), new ActionDescriptor());
            var authorizationFilterContext = new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());

            if (authorizationFilters.TryGetValue(invoker.MethodInfo, out var filters))
            {
                RunAuthorizationFilters(authorizationFilterContext, filters);
            }
            else
            {
                var attributes = invoker.MethodInfo.GetCustomAttributes(true);
                filters = attributes
                    .Where(x => x is IAuthorizationFilter)
                    .Select(x => x as IAuthorizationFilter);
                authorizationFilters.Add(invoker.MethodInfo, filters);
                RunAuthorizationFilters(authorizationFilterContext, filters);
            }
        }

        private void RunAuthorizationFilters(AuthorizationFilterContext authorizationFilterContext, IEnumerable<IAuthorizationFilter> filters)
        {
            foreach (var filter in filters)
            {
                filter.OnAuthorization(authorizationFilterContext);
            }
        }


        internal Task Cors(HttpContext context)
        {
            AddCors(context);
            context.Response.Headers.Add("Connection", "keep-alive");

            return Task.CompletedTask;
        }

        private static void AddCors(HttpContext context)
        {
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Add("Access-Control-Allow-Methods", "POST");
            context.Response.Headers.Add("Access-Control-Allow-Headers", "content-type");
        }
    }
}
