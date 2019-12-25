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
            var appPartsMgr = routes.ServiceProvider.GetRequiredService<IApplicationPartManager>();

            var grainInterfaceFeature = appPartsMgr.CreateAndPopulateFeature<GrainInterfaceFeature>();

            var dataEntitiesToMap = DiscoverDataEntitiesToMap(grainInterfaceFeature);

            int routesCreated = 0;
            // Map each grain type to a route based on the attributes
            foreach (var serviceType in dataEntitiesToMap)
            {
                routesCreated += MapDataEntityToRoute(routes, serviceType, prefix);
            }
            
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

        private static int MapDataEntityToRoute(IEndpointRouteBuilder routes, Type grainType, string prefix)
        {            
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
            
            routesRegistered = MapCustomMethods(dataEntityName, keyStrategy, routes, prefix, methods, routesRegistered);
            routesRegistered = MapCrudMethods(dataEntityName, grainType, keyStrategy, routes, prefix, routesRegistered);

            return routesRegistered;
        }

        private static int MapCustomMethods(string dataEntityName,
            KeyStrategy keyStrategy,
            IEndpointRouteBuilder routeBuilder,
            string prefix,            
            MethodInfo[] methods,
            int routesRegistered)
        {           
            foreach (var method in methods)
            {
                DataEntityMethodDispatcher.Register(routeBuilder, 
                    prefix, 
                    dataEntityName, 
                    keyStrategy,
                    method.DeclaringType,
                    method);

                routesRegistered++;         
            }

            return routesRegistered;
        }

        private static int MapCrudMethods(string dataEntityName, 
            Type declaringType, 
            KeyStrategy keyStrategy, 
            IEndpointRouteBuilder routeBuilder, 
            string prefix,             
            int routesRegistered)
        {            
            var dataEntityType = (
                from iType in declaringType.GetInterfaces()
                where iType.IsGenericType
                        && iType.GetGenericTypeDefinition() == typeof(IDataEntity<>)
                select iType.GetGenericArguments()[0]).First();            

            void Register(HttpMethod httpMethod) {
                DataEntityCrudDispatcher.Register(routeBuilder, 
                    prefix, 
                    dataEntityName, 
                    keyStrategy,
                    declaringType,                    
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

    }
}
