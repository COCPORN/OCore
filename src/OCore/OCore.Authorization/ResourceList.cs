using OCore.Authorization.Abstractions;
using OCore.Entities.Data;
using OCore.Services;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OCore.Authorization
{

    public abstract class Resource
    {
        /// <summary>
        /// The specialized name of the resource
        /// </summary>
        public string ResourceName { get; private set; }

        /// <summary>
        /// The base resource that holds this specialization
        /// </summary>
        public string BaseResource { get; private set; }

        /// <summary>
        /// Permissions needed to reach this concrete resource
        /// </summary>
        public Permissions Permissions { get; private set; }

        public Resource(string resourceName, string baseResource, Permissions permission)
        {
            ResourceName = resourceName;
            Permissions = permission;
            BaseResource = baseResource;
        }
    }

    public class ServiceResource : Resource
    {
        public ServiceResource(string resourceName, string baseResource, Permissions permission) 
            : base(resourceName, baseResource, permission)
        {
        }
    }

    public class DataEntityResource : Resource
    {        
        public DataEntityResource(string resourceName, string baseResource, Permissions permission) 
            : base(resourceName, baseResource, permission)
        {            
        }
    }

    public static class ResourceEnumerator
    {
        static List<Resource> resources;

        public static List<Resource> Resources
        {
            get
            {
                if (resources == null)
                {
                    resources = FindServiceResources()
                        .Concat(FindDataEntityResources())
                        .ToList();
                }
                return resources;
            }
        }

        static List<Resource> FindDataEntityResources()
        {
            var interfaces = AppDomain
              .CurrentDomain
              .GetAssemblies()
              .SelectMany(x => x.GetTypes())
              .Where(x => typeof(IDataEntity).IsAssignableFrom(x))
              .SelectMany(x => x.GetInterfaces())
              .Where(x => typeof(IDataEntity).IsAssignableFrom(x));

            var dataEntityInterfaces = interfaces
              .Where(x => x.GetCustomAttributes(true).Where(z => z is DataEntityAttribute).Any());

            var dataResourcesFromMethod = dataEntityInterfaces
                .SelectMany(x => x.GetMethods()
                    .Select(y => GetDataResourceFromMethod(x, y))).ToList();

            var dataResourcesFromCrud = dataEntityInterfaces.
                SelectMany(x => GetDataResourceFromCrud(x)).ToList();
            
            return dataResourcesFromMethod.Concat(dataResourcesFromCrud).ToList();            
        }


        static List<Resource> FindServiceResources()
        {
            return AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => x.IsSubclassOf(typeof(Service)))
                .SelectMany(x => x.GetInterfaces())
                .Where(x => x.GetCustomAttributes(true).Where(z => z is ServiceAttribute).Any())
                .SelectMany(x => x.GetMethods()
                    .Select(y => GetServiceResourceFromMethod(x, y))).ToList();
        }

        private static Resource GetServiceResourceFromMethod(Type type, MethodInfo methodInfo)
        {
            var authorizeAttribute = methodInfo.GetCustomAttribute<AuthorizeAttribute>();
            if (authorizeAttribute != null)
            {
                return new ServiceResource(CreateServiceResourceName(type, methodInfo), ServiceBaseResourceName(type), authorizeAttribute.Permissions);
            }
            else
            {
                return new ServiceResource(CreateServiceResourceName(type, methodInfo), ServiceBaseResourceName(type), Permissions.All);
            }
        }

        private static Resource GetDataResourceFromMethod(Type type, MethodInfo methodInfo)
        {
            var authorizeAttribute = methodInfo.GetCustomAttribute<AuthorizeAttribute>();
            if (authorizeAttribute != null)
            {
                return new DataEntityResource(CreateDataResourceName(type, methodInfo), DataBaseResourceName(type), authorizeAttribute.Permissions);
            }
            else
            {
                return new DataEntityResource(CreateDataResourceName(type, methodInfo), DataBaseResourceName(type), Permissions.All);
            }
        }

        private static string CreateServiceResourceName(Type type, MethodInfo method)
        {
            var serviceAttribute = type
                .GetCustomAttributes(true)
                .Where(z => z is ServiceAttribute)
                .Select(x => x as ServiceAttribute)
                .First();
            return $"{serviceAttribute.Name}/{method.Name}";
        }

        private static string CreateDataResourceName(Type type, MethodInfo method)
        {
            var dataEntityAttribute = type
                .GetCustomAttributes(true)
                .Where(z => z is DataEntityAttribute)
                .Select(x => x as DataEntityAttribute)
                .First();

            if (dataEntityAttribute.DataEntityMethods.HasFlag(DataEntityMethods.Commands))
            {
                return $"{dataEntityAttribute.Name}/{method.Name}";
            } else
            {
                return null;
            }
        }

        static string DataBaseResourceName(Type type)
        {
            var dataEntityAttribute = type
                .GetCustomAttributes(true)
                .Where(z => z is DataEntityAttribute)
                .Select(x => x as DataEntityAttribute)
                .First();

            return dataEntityAttribute.Name;
        }

        static string ServiceBaseResourceName(Type type)
        {
            var serviceAttribute = type
             .GetCustomAttributes(true)
             .Where(z => z is ServiceAttribute)
             .Select(x => x as ServiceAttribute)
             .First();

            return serviceAttribute.Name;
        }

        private static List<Resource> GetDataResourceFromCrud(Type type)
        {
            var dataEntityAttribute = type
                .GetCustomAttributes(true)
                .Where(z => z is DataEntityAttribute)
                .Select(x => x as DataEntityAttribute)
                .FirstOrDefault();

            var dataResources = new List<Resource>();

            if (dataEntityAttribute.DataEntityMethods.HasFlag(DataEntityMethods.Create))
            {
                dataResources.Add(new DataEntityResource($"{dataEntityAttribute.Name}/Create", DataBaseResourceName(type), Permissions.Write));
            }

            if (dataEntityAttribute.DataEntityMethods.HasFlag(DataEntityMethods.Read))
            {
                dataResources.Add(new DataEntityResource($"{dataEntityAttribute.Name}/Read", DataBaseResourceName(type), Permissions.Read));
            }

            if (dataEntityAttribute.DataEntityMethods.HasFlag(DataEntityMethods.Update))
            {
                dataResources.Add(new DataEntityResource($"{dataEntityAttribute.Name}/Update", DataBaseResourceName(type), Permissions.Write));
                dataResources.Add(new DataEntityResource($"{dataEntityAttribute.Name}/Upsert", DataBaseResourceName(type), Permissions.Write));
            }

            if (dataEntityAttribute.DataEntityMethods.HasFlag(DataEntityMethods.Delete))
            {
                dataResources.Add(new DataEntityResource($"{dataEntityAttribute.Name}/Delete", DataBaseResourceName(type), Permissions.Write));
            }

            return dataResources;
        }

    }

}
