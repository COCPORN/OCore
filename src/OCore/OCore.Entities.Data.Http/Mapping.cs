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
using Orleans.Runtime;

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

            var keyStrategy = KeyStrategy.Identity;

            if (dataEntityAttribute != null)
            {
                dataEntityName = dataEntityAttribute.Name;
                keyStrategy = dataEntityAttribute.KeyStrategy;
            }

            Type declaringType;
            (routesRegistered, declaringType) = MapCustomMethods(dataEntityName, keyStrategy, routes, prefix, dispatcher, methods, routesRegistered);

            routesRegistered = MapCrudMethods(dataEntityName, declaringType, keyStrategy, routes, prefix, dispatcher, routesRegistered);

            return routesRegistered;
        }

        private static (int, Type) MapCustomMethods(string dataEntityName,
            KeyStrategy keyStrategy,
            IEndpointRouteBuilder routes,
            string prefix,
            DataEntityRouter dispatcher,
            MethodInfo[] methods,
            int routesRegistered)
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

        private static int MapCrudMethods(string dataEntityName, Type declaringType, KeyStrategy keyStrategy, IEndpointRouteBuilder routes, string prefix, DataEntityRouter dispatcher, int routesRegistered)
        {            
            Type concreteGrainType = FindConcreteGrainType(declaringType);

            var interfaces = concreteGrainType.GetInterfaces();

            var dataEntityType = (
                from iType in declaringType.GetInterfaces()
                where iType.IsGenericType
                        && iType.GetGenericTypeDefinition() == typeof(IDataEntity<>)
                select iType.GetGenericArguments()[0]).First();

            var interfaceType = typeof(IDataEntity<>).MakeGenericType(declaringType);

            void Register(HttpMethod httpMethod) {
                DataEntityCrudDispatcher.Register(routes, 
                    prefix, 
                    dataEntityName, 
                    keyStrategy, 
                    interfaceType,
                    dataEntityType, 
                    httpMethod);
            }

            Register(HttpMethod.Post);
            Register(HttpMethod.Get);
            Register(HttpMethod.Put);
            Register(HttpMethod.Delete);

            routesRegistered += 4;
            return routesRegistered;
        }

        private static Type FindConcreteGrainType(Type declaringType)
        {
            // We have the interface. Let's find the class that actually implements
            // the grain
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => declaringType.IsAssignableFrom(p)
                            && p != declaringType
                            && p.Name.Contains("CodeGen") == false)
                .Single();
        }

        private static RoutePattern MapRouteToGrainType(string dataEntityName, string prefix, Type[] interfaces)
        {
            RoutePattern routePattern;
            if (interfaces.Contains(typeof(IGrainWithGuidKey)))
            {
                routePattern = RoutePatternFactory.Parse($"{prefix}/{dataEntityName}/{{entityId}}");
            }
            else if (interfaces.Contains(typeof(IGrainWithStringKey)))
            {
                routePattern = RoutePatternFactory.Parse($"{prefix}/{dataEntityName}/{{entityId}}");
            }
            else
            {
                throw new InvalidOperationException("I cannot understand which type of grain type this is");
            }

            return routePattern;
        }

    }
}
