using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Orleans.ApplicationParts;
using Orleans.Metadata;
using Orleans;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Builder;

namespace OCore.Entities.Data.Http
{
    public static class Mapping 
    {
        public static IEndpointRouteBuilder MapDataEntities(this IEndpointRouteBuilder routes, string prefix = "")
        {
            var dispatcher = routes.ServiceProvider.GetRequiredService<DataEntityRouter>();
            var logger = routes.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<DataEntityRouter>();
            var appPartsMgr = routes.ServiceProvider.GetRequiredService<IApplicationPartManager>();

            var grainInterfaceFeature = appPartsMgr.CreateAndPopulateFeature<GrainInterfaceFeature>();

            var dataEntitiesToMap = DiscoverDataEntitiesToMap(grainInterfaceFeature);

            int routesCreated = 0;
            // Map each grain type to a route based on the attributes
            foreach (var serviceType in dataEntitiesToMap)
            {
                routesCreated += MapDataEntityToRoute(routes, serviceType, prefix, dispatcher, logger);
            }

            logger.LogInformation($"{routesCreated} route(s) were created for grains.");
            return routes;
        }

        private static List<Type> DiscoverDataEntitiesToMap(GrainInterfaceFeature grainInterfaceFeature)
        {
            var grainTypesToMap = new List<Type>();

            foreach (var grainInterfaceMetadata in grainInterfaceFeature.Interfaces)
            {
                var grainType = grainInterfaceMetadata.InterfaceType;

                if (grainType.GetInterfaces().Contains(typeof(IDataEntity)))
                {
                    grainTypesToMap.Add(grainType);
                }
            }

            return grainTypesToMap;
        }

        private static int MapDataEntityToRoute(IEndpointRouteBuilder routes, Type grainType, string prefix, DataEntityRouter dispatcher, ILogger<DataEntityRouter> logger)
        {
            logger.LogInformation($"Mapping routes for service '{grainType.FullName}'");
            
            var methods = grainType.GetMethods();
            int routesRegistered = 0;

            foreach (var method in methods)
            {
                var routePattern = RoutePatternFactory.Parse($"{prefix}");
                var route = routes.MapPost(routePattern.RawText, dispatcher.Dispatch);
                //var cors = routes.MapMethods(routePattern.RawText, new string[] { "OPTIONS" }, dispatcher.Cors);

                dispatcher.RegisterRoute(routePattern.RawText, method);

                routesRegistered++;
            }

            return routesRegistered;
        }

    }
}
