using Microsoft.AspNetCore.Http;
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
            var endpoint = (RouteEndpoint)context.GetEndpoint();
            var pattern = endpoint.RoutePattern;

            var invoker = routes[pattern.RawText];

            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");

            var grain = clusterClient.GetGrain(invoker.GrainType, 0);
            if (grain == null)
            {
                // We only faw here if the grainId is mal formed
                context.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                return Task.CompletedTask;
            }

            return invoker.Invoke(grain, context);
        }

        internal Task Cors(HttpContext context)
        {
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Add("Access-Control-Allow-Methods", "POST");
            context.Response.Headers.Add("Access-Control-Allow-Headers", "content-type");
            context.Response.Headers.Add("Connection", "keep-alive");

            return Task.CompletedTask;
        }
    }
}
