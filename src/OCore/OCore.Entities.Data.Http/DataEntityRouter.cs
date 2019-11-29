using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OCore.Http;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OCore.Entities.Data.Http
{
    public class DataEntityRouter : Router
    {

        IClusterClient clusterClient;
        IServiceProvider serviceProvider;
        ILogger logger;

        public DataEntityRouter(IClusterClient clusterClient,
                IServiceProvider serviceProvider,
                ILogger<DataEntityRouter> logger)
        {
            this.clusterClient = clusterClient;
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        private readonly Dictionary<string, GrainInvoker> routes = new Dictionary<string, GrainInvoker>(StringComparer.InvariantCultureIgnoreCase);

        public void RegisterRoute(string pattern, MethodInfo methodInfo)
        {
            CheckGrainType(methodInfo.DeclaringType);
            routes.Add(pattern, new DataEntityGrainInvoker(serviceProvider, methodInfo));
        }

        private void CheckGrainType(Type grainInterfaceType)
        {
            var ifaces = grainInterfaceType.GetInterfaces();
            if (ifaces.Contains(typeof(IDataEntity)) == false)
            {
                throw new InvalidOperationException("Data entities must implement IDataEntity");
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

    }
}
