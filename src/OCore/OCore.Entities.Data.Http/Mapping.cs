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
using System.Reflection;

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
            logger.LogInformation($"Mapping routes for Data Entity '{grainType.FullName}'");

            var methods = grainType.GetMethods();
            int routesRegistered = 0;

            var dataEntityName = grainType.FullName;

            var dataEntityAttribute = (DataEntityAttribute)grainType.GetCustomAttributes(true).Where(attr => attr.GetType() == typeof(DataEntityAttribute)).SingleOrDefault();

            if (dataEntityAttribute != null)
            {
                dataEntityName = dataEntityAttribute.Name;
            }

            Type declaringType;
            (routesRegistered, declaringType) = MapCustomMethods(dataEntityName, routes, prefix, dispatcher, methods, routesRegistered);

            routesRegistered = MapCrudMethods(dataEntityName, declaringType, routes, prefix, dispatcher, routesRegistered);

            return routesRegistered;
        }

        private static (int, Type) MapCustomMethods(string dataEntityName, IEndpointRouteBuilder routes, string prefix, DataEntityRouter dispatcher, System.Reflection.MethodInfo[] methods, int routesRegistered)
        {
            Type declaringType = null;
            foreach (var method in methods)
            {
                var routePattern = RoutePatternFactory.Parse($"{prefix}/{dataEntityName}/{{entityId}}/{method.Name}");
                var route = routes.MapPost(routePattern.RawText, dispatcher.DispatchCustomOperation);
                //var cors = routes.MapMethods(routePattern.RawText, new string[] { "OPTIONS" }, dispatcher.Cors);

                dispatcher.RegisterCommandRoute(routePattern.RawText, method);

                routesRegistered++;
                declaringType = method.DeclaringType;
            }

            return (routesRegistered, declaringType);
        }

        private static int MapCrudMethods(string dataEntityName, Type declaringType, IEndpointRouteBuilder routes, string prefix, DataEntityRouter dispatcher, int routesRegistered)
        {

            var routePattern = RoutePatternFactory.Parse($"{prefix}/{dataEntityName}/{{entityId}}");
            routes.MapPost(routePattern.RawText, ctx => dispatcher.DispatchCrudOperation(ctx, HttpMethod.Post));
            routes.MapGet(routePattern.RawText, ctx => dispatcher.DispatchCrudOperation(ctx, HttpMethod.Get));
            routes.MapPut(routePattern.RawText, ctx => dispatcher.DispatchCrudOperation(ctx, HttpMethod.Put));
            routes.MapDelete(routePattern.RawText, ctx => dispatcher.DispatchCrudOperation(ctx, HttpMethod.Delete));

            //var cors = routes.MapMethods(routePattern.RawText, new string[] { "OPTIONS" }, dispatcher.Cors);            

            var dataEntityType = (
                from iType in declaringType.GetInterfaces()
                where iType.IsGenericType
                        && iType.GetGenericTypeDefinition() == typeof(IDataEntity<>)
                select iType.GetGenericArguments()[0]).First();

            var getMethod = typeof(IDataEntity<>).MakeGenericType(dataEntityType).GetMethod("Read");
            var putMethod = typeof(IDataEntity<>).MakeGenericType(dataEntityType).GetMethod("Upsert");
            var postMethod = typeof(IDataEntity<>).MakeGenericType(dataEntityType).GetMethod("Create");
            var deleteMethod = typeof(IDataEntity<>).MakeGenericType(dataEntityType).GetMethod("Delete");

            var interfaceType = typeof(IDataEntity<>).MakeGenericType(declaringType);

            dispatcher.RegisterCrudRoute($"{routePattern.RawText}:{HttpMethod.Post}", postMethod, declaringType, HttpMethod.Post, interfaceType, dataEntityType);
            dispatcher.RegisterCrudRoute($"{routePattern.RawText}:{HttpMethod.Get}", getMethod, declaringType, HttpMethod.Get, interfaceType, dataEntityType);
            dispatcher.RegisterCrudRoute($"{routePattern.RawText}:{HttpMethod.Put}", putMethod, declaringType, HttpMethod.Put, interfaceType, dataEntityType);
            dispatcher.RegisterCrudRoute($"{routePattern.RawText}:{HttpMethod.Delete}", deleteMethod, declaringType, HttpMethod.Delete, interfaceType, dataEntityType);

            routesRegistered += 4;

            return routesRegistered;
        }


    }
}
