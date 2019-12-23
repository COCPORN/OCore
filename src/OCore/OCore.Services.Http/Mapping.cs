using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OCore.Core;
using Orleans;
using Orleans.ApplicationParts;
using Orleans.Concurrency;
using Orleans.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace OCore.Services.Http
{
    public static class Mapping
    {
        public static IEndpointRouteBuilder MapServices(this IEndpointRouteBuilder routes, string prefix = "")
        {
            var dispatcher = routes.ServiceProvider.GetRequiredService<ServiceRouter>();
            var logger = routes.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ServiceRouter>();
            var appPartsMgr = routes.ServiceProvider.GetRequiredService<IApplicationPartManager>();            

            var grainInterfaceFeature = appPartsMgr.CreateAndPopulateFeature<GrainInterfaceFeature>();

            var servicesToMap = DiscoverServicesToMap(grainInterfaceFeature);

            int routesCreated = 0;
            // Map each grain type to a route based on the attributes
            foreach (var serviceType in servicesToMap)
            {
                routesCreated += MapServiceToRoute(routes, serviceType, prefix, dispatcher, logger);
            }            

            logger.LogInformation($"{routesCreated} route(s) were created for grains.");
            return routes;
        }

        private static int MapServiceToRoute(IEndpointRouteBuilder routes, Type grainType, string prefix, ServiceRouter dispatcher, ILogger<ServiceRouter> logger)
        {
            logger.LogInformation($"Mapping routes for service '{grainType.FullName}'");

            var internalAttribute = (InternalAttribute)grainType.GetCustomAttributes(true).Where(attr => attr.GetType() == typeof(InternalAttribute)).SingleOrDefault();

            if (internalAttribute != null)
            {
                return 0;
            }

            var serviceAttribute = (ServiceAttribute)grainType.GetCustomAttributes(true).Where(attr => attr.GetType() == typeof(ServiceAttribute)).SingleOrDefault();

            var methods = grainType.GetMethods();
            int routesRegistered = 0;

            foreach (var method in methods)
            {
                internalAttribute = (InternalAttribute)grainType.GetCustomAttributes(true).Where(attr => attr.GetType() == typeof(InternalAttribute)).SingleOrDefault();

                if (internalAttribute != null) continue;

                var routePattern = RoutePatternFactory.Parse($"{prefix}/{serviceAttribute.Name}/{method.Name}");
                var route = routes.MapPost(routePattern.RawText, dispatcher.Dispatch);                

                dispatcher.RegisterRoute(routePattern.RawText, method);    
                              
                routesRegistered++;
            }

            return routesRegistered;
        }

        private static List<Type> DiscoverServicesToMap(GrainInterfaceFeature grainInterfaceFeature)
        {
            var grainTypesToMap = new List<Type>();

            foreach (var grainInterfaceMetadata in grainInterfaceFeature.Interfaces)
            {
                var grainType = grainInterfaceMetadata.InterfaceType;

                var customAttributes = grainType.GetCustomAttributes(true);

                if (customAttributes.Any(x => x.GetType() == typeof(ServiceAttribute)))
                {
                    grainTypesToMap.Add(grainType);
                }
            }

            return grainTypesToMap;
        }

    }
}
