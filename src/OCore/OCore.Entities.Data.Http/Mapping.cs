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
using OCore.Authorization.Abstractions;
using OCore.Core;

namespace OCore.Entities.Data.Http
{
    public static class Mapping
    {
        public static IEndpointRouteBuilder MapDataEntities(this IEndpointRouteBuilder routes, string prefix = "")
        {
            var appPartsMgr = routes.ServiceProvider.GetRequiredService<IApplicationPartManager>();
            var payloadCompleter = routes.ServiceProvider.GetRequiredService<IPayloadCompleter>();

            var grainInterfaceFeature = appPartsMgr.CreateAndPopulateFeature<GrainInterfaceFeature>();

            var dataEntitiesToMap = DiscoverDataEntitiesToMap(grainInterfaceFeature);

            int routesCreated = 0;
            // Map each grain type to a route based on the attributes
            foreach (var serviceType in dataEntitiesToMap)
            {
                routesCreated += MapDataEntityToRoute(routes, serviceType, prefix, payloadCompleter);
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

        private static int MapDataEntityToRoute(IEndpointRouteBuilder routes, Type grainType, string prefix, IPayloadCompleter payloadCompleter)
        {
            var methods = grainType.GetMethods();
            int routesRegistered = 0;

            var dataEntityName = grainType.FullName;

            var dataEntityAttribute = (DataEntityAttribute)grainType.GetCustomAttributes(true).Where(attr => attr.GetType() == typeof(DataEntityAttribute)).SingleOrDefault();

            var keyStrategy = KeyStrategy.Identity;
            var dataEntityMethods = DataEntityMethods.All;
            var maxFanoutLimit = 0;

            if (dataEntityAttribute != null)
            {
                dataEntityName = dataEntityAttribute.Name;
                keyStrategy = dataEntityAttribute.KeyStrategy;
                dataEntityMethods = dataEntityAttribute.DataEntityMethods;
                maxFanoutLimit = dataEntityAttribute.MaxFanoutLimit;
            }

            routesRegistered = MapCustomMethods(dataEntityName, keyStrategy, maxFanoutLimit, routes, payloadCompleter, prefix, methods, routesRegistered);
            routesRegistered = MapCrudMethods(dataEntityName, grainType, keyStrategy, maxFanoutLimit, dataEntityMethods, routes, payloadCompleter, prefix, routesRegistered);

            return routesRegistered;
        }

        private static int MapCustomMethods(string dataEntityName,
            KeyStrategy keyStrategy,
            int maxFanoutLimit,
            IEndpointRouteBuilder routeBuilder,
            IPayloadCompleter payloadCompleter,
            string prefix,
            MethodInfo[] methods,
            int routesRegistered)
        {
            foreach (var method in methods)
            {
                var internalAttribute = method.GetCustomAttribute<InternalAttribute>(true);

                if (internalAttribute == null)
                {
                    DataEntityMethodDispatcher.Register(routeBuilder,
                        prefix,
                        dataEntityName,
                        keyStrategy,
                        maxFanoutLimit,
                        payloadCompleter,
                        method.DeclaringType,
                        method);

                    routesRegistered++;
                }
            }

            return routesRegistered;
        }

        private static int MapCrudMethods(string dataEntityName,
            Type declaringType,
            KeyStrategy keyStrategy,
            int maxFanoutLimit,
            DataEntityMethods dataEntityMethods,
            IEndpointRouteBuilder routeBuilder,
            IPayloadCompleter payloadCompleter,
            string prefix,
            int routesRegistered)
        {
            var dataEntityType = (
                from iType in declaringType.GetInterfaces()
                where iType.IsGenericType
                        && iType.GetGenericTypeDefinition() == typeof(IDataEntity<>)
                select iType.GetGenericArguments()[0]).First();

            void Register(HttpMethod httpMethod)
            {
                DataEntityCrudDispatcher.Register(routeBuilder,
                    prefix,
                    dataEntityName,
                    keyStrategy,
                    maxFanoutLimit,
                    declaringType,
                    dataEntityType,
                    payloadCompleter,
                    httpMethod);
            }


            if (dataEntityMethods.HasFlag(DataEntityMethods.Create))
            {
                Register(HttpMethod.Post);
                routesRegistered++;
            }

            if (dataEntityMethods.HasFlag(DataEntityMethods.Read))
            {
                Register(HttpMethod.Get);
                routesRegistered++;
            }

            if (dataEntityMethods.HasFlag(DataEntityMethods.Update))
            {
                Register(HttpMethod.Put);
                routesRegistered++;
            }

            if (dataEntityMethods.HasFlag(DataEntityMethods.Delete))
            {
                Register(HttpMethod.Delete);
                routesRegistered++;
            }

            return routesRegistered;
        }

    }
}
