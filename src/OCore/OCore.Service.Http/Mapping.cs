using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.ApplicationParts;
using Orleans.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace OCore.Service
{
    public static class Mapping
    {
        public static IEndpointRouteBuilder MapServices(this IEndpointRouteBuilder routes, string prefix = "")
        {
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                prefix = $"{prefix}/";
            }
            else
            {
                prefix = "/";
            }

            var dispatcher = routes.ServiceProvider.GetRequiredService<ServiceRouter>();
            var logger = routes.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ServiceRouter>();
            var appPartsMgr = routes.ServiceProvider.GetRequiredService<IApplicationPartManager>();

            var grainInterfaceFeature = appPartsMgr.CreateAndPopulateFeature<GrainInterfaceFeature>();
            var grainTypesToMap = DiscoverGrainTypesToMap(grainInterfaceFeature);

            int routesCreated = 0;
            // Map each grain type to a route based on the attributes
            foreach (var grainType in grainTypesToMap)
            {
                routesCreated += MapServiceToRoute(routes, grainType, prefix, dispatcher, logger);
            }

            logger.LogInformation($"{routesCreated} route(s) were created for grains.");
            return routes;
        }

        private static int MapServiceToRoute(IEndpointRouteBuilder routes, Type grainType, string prefix, ServiceRouter dispatcher, ILogger<ServiceRouter> logger)
        {
            logger.LogInformation($"Mapping routes for grain '{grainType.FullName}'...");

            int routesRegistered = 0;

            return routesRegistered;
        }

        private static List<Type> DiscoverGrainTypesToMap(GrainInterfaceFeature grainInterfaceFeature)
        {
            var grainTypesToMap = new List<Type>();

            foreach (var grainInterfaceMetadata in grainInterfaceFeature.Interfaces)
            {
                var grainType = grainInterfaceMetadata.InterfaceType;

                // Only add to the list grains that either have the top-level route attribute or has one of the method attributes
                if (grainType.GetCustomAttributes(true).Contains(typeof(ServiceAttribute)))
                {
                    grainTypesToMap.Add(grainType);
                }
            }

            return grainTypesToMap;
        }

    }
}
