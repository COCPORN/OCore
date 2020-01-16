using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using OCore.Http;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OCore.Services.Http
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

        readonly Dictionary<string, GrainInvoker> routes = new Dictionary<string, GrainInvoker>(StringComparer.InvariantCultureIgnoreCase);

        public void RegisterRoute(string pattern, MethodInfo methodInfo)
        {
            CheckGrainType(methodInfo.DeclaringType);
            routes.Add(pattern, new ServiceGrainInvoker(serviceProvider, methodInfo.DeclaringType, methodInfo));
        }

        private void CheckGrainType(Type grainInterfaceType)
        {
            var interfaces = grainInterfaceType.GetInterfaces();
            if (interfaces.Contains(typeof(IGrainWithIntegerKey)) == false)
            {
                throw new InvalidOperationException("Service is not of correct type");
            }
        }

        public async Task Dispatch(HttpContext context)
        {
            var endpoint = (RouteEndpoint)context.GetEndpoint();
            var pattern = endpoint.RoutePattern;

            var invoker = routes[pattern.RawText];
            context.RunAuthorizationFilters(invoker);
            context.RunActionFiltersExecuting(invoker);
            await context.RunAsyncActionFilters(invoker, async (context) =>
            {
                var grain = clusterClient.GetGrain(invoker.GrainType, 0);
                if (grain == null)
                {
                    await context.SetStatusCode(System.Net.HttpStatusCode.BadRequest);
                    return;
                }


                await invoker.Invoke(grain, context);
                context.RunActionFiltersExecuted(invoker);
            });
        }


    }
}
