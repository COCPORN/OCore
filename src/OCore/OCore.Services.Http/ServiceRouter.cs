using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OCore.Http;
using OCore.Services.Http.Options;
using Orleans;
using Orleans.Runtime;
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
        readonly HttpOptions httpOptions;

        public ServiceRouter(IClusterClient clusterClient,
            IServiceProvider serviceProvider,
            ILogger<ServiceRouter> logger,
            IOptions<HttpOptions> options)
        {
            this.clusterClient = clusterClient;
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            this.httpOptions = options.Value;
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

            RequestContext.Set("D:RequestSource", "HTTP");
            RequestContext.Set("D:GrainName", pattern.RawText);

            var correlationIdKeyName = httpOptions.CorrelationIdHeader;

            if (correlationIdKeyName == null)
            {
                correlationIdKeyName = "correlationid";
            }

            // I have a feeling this can be improved by not using FirstOrDefault
            var correlationId = context.Request.Headers.FirstOrDefault(x => x.Key == correlationIdKeyName).Value.ToString();

            if (string.IsNullOrEmpty(correlationId) == true)
            {
                correlationId = Guid.NewGuid().ToString();
            }

            RequestContext.Set("D:CorrelationId", correlationId);

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
