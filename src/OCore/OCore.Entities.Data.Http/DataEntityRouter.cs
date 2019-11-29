using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OCore.Http;
using Orleans;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
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

        public void RegisterCommandRoute(string pattern, MethodInfo methodInfo)
        {
            routes.Add(pattern, new DataEntityGrainInvoker(serviceProvider, methodInfo.DeclaringType, methodInfo.DeclaringType, methodInfo, null));
        }

        public void RegisterCrudRoute(string pattern, 
            MethodInfo methodInfo, 
            Type grainType, 
            HttpMethod httpMethod, 
            Type interfaceType,
            Type entityType)
        {
            routes.Add(pattern, new DataEntityGrainInvoker(serviceProvider, grainType, interfaceType, methodInfo, entityType)
            {
                IsCrudOperation = true,
                HttpMethod = httpMethod,                
            });
        }

        public Task DispatchCustomOperation(HttpContext context)
        {
            AddCors(context);
            var endpoint = (RouteEndpoint)context.GetEndpoint();
            var pattern = endpoint.RoutePattern;

            var invoker = routes[pattern.RawText];
            RunAuthorizationFilters(context, invoker);
            RunActionFilters(context, invoker);

            var getGrainId = GetGrainId(context);

            var grain = clusterClient.GetGrain(invoker.GrainType, getGrainId);
            if (grain == null)
            {
                context.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                return Task.CompletedTask;
            }

            return invoker.Invoke(grain, context);
        }

        private string GetGrainId(HttpContext context)
        {
            return context.Request.RouteValues["entityId"].ToString();
        }

        public Task DispatchCrudOperation(HttpContext context, HttpMethod httpMethod)
        {
            AddCors(context);
            var endpoint = (RouteEndpoint)context.GetEndpoint();
            var pattern = endpoint.RoutePattern;

            var invoker = routes[$"{pattern.RawText}:{httpMethod}"];

             
            RunAuthorizationFilters(context, invoker);
            RunActionFilters(context, invoker);

            var grain = clusterClient.GetGrain(invoker.GrainType, GetGrainId(context));
            if (grain == null)
            {
                context.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                return Task.CompletedTask;
            }

            return invoker.Invoke(grain, context);
        }

    }
}
