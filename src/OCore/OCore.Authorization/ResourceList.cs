using OCore.Authorization.Abstractions;
using OCore.Core;
using OCore.Entities.Data;
using OCore.Services;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OCore.Authorization
{

    public abstract class Resource
    {
        /// <summary>
        /// The specialized name of the resource
        /// </summary>
        public string ResourcePath { get; private set; }

        /// <summary>
        /// The base resource that holds this specialization
        /// </summary>
        public string BaseResource { get; private set; }

        /// <summary>
        /// Permissions needed to reach this concrete resource
        /// </summary>
        public Permissions Permissions { get; private set; }


        public MethodInfo MethodInfo { get; private set; }

        public bool IsPublic { get; private set; }

        public Resource(string resourceName,
            string baseResource,
            Permissions permission,
            MethodInfo methodInfo,
            bool isPublic)
        {
            ResourcePath = resourceName;
            Permissions = permission;
            BaseResource = baseResource;
            MethodInfo = methodInfo;
            IsPublic = isPublic;
        }
    }

    [Serializable]
    [GenerateSerializer]
    public class ServiceResource : Resource
    {
        public ServiceResource(string resourceName,
            string baseResource,
            Permissions permission,
            MethodInfo methodInfo,
            bool isPublic)
            : base(resourceName,
                  baseResource,
                  permission,
                  methodInfo,
                  isPublic)
        {
        }
    }

    [Serializable]
    [GenerateSerializer]
    public class DataEntityResource : Resource
    {
        [Id(0)]
        public DataEntityAttribute Attribute { get; private set; }

        public DataEntityResource(string resourceName,
            string baseResource,
            Permissions permission,
            MethodInfo methodInfo,
            DataEntityAttribute attribute,
            bool isPublic)
            : base(resourceName, baseResource, permission, methodInfo, isPublic)
        {
            Attribute = attribute;
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

        public static List<Resource> PublicResources
        {
            get
            {
                return FindServiceResources(false)
                        .Concat(FindDataEntityResources(false))
                        .ToList();
            }
        }

        static List<Resource> FindDataEntityResources(bool includePrivate = true)
        {
            var interfaces = AppDomain
              .CurrentDomain
              .GetAssemblies()
              .SelectMany(x => x.GetTypes())
              .Where(x => typeof(IDataEntity).IsAssignableFrom(x))
              .SelectMany(x => x.GetInterfaces())
              .Where(x => typeof(IDataEntity).IsAssignableFrom(x))
              .Distinct();

            var dataEntityInterfaces = interfaces
              .Where(x => x.GetCustomAttributes(true).Where(z => z is DataEntityAttribute).Any());

            dataEntityInterfaces = dataEntityInterfaces
                .Where(x => includePrivate == true
                       || x.GetCustomAttributes(true).Where(z => z is InternalAttribute).Any() == false);

            var dataResourcesFromMethod = dataEntityInterfaces
                .SelectMany(x => x.GetMethods()
                    .Select(y => GetDataResourceFromMethod(x, y))).ToList();

            var dataResourcesFromCrud = dataEntityInterfaces.
                SelectMany(x => GetDataResourceFromCrud(x)).ToList();

            return dataResourcesFromMethod.Concat(dataResourcesFromCrud).ToList();
        }


        static List<Resource> FindServiceResources(bool includePrivate = true)
        {
            return AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => x.IsSubclassOf(typeof(Service)))
                .SelectMany(x => x.GetInterfaces())
                .Where(x => x.GetCustomAttributes(true).Where(z => z is ServiceAttribute).Any())
                .Where(x => includePrivate == true || x.GetCustomAttributes(true).Where(z => z is InternalAttribute).Any() == false)
                .SelectMany(x => x.GetMethods()
                    .Select(y => GetServiceResourceFromMethod(x, y))).ToList();
        }

        private static Resource GetServiceResourceFromMethod(Type type, MethodInfo methodInfo)
        {
            var authorizeAttribute = methodInfo.GetCustomAttribute<AuthorizeAttribute>();
            var isPublic = methodInfo.GetCustomAttribute<InternalAttribute>() == null;

            if (authorizeAttribute != null)
            {
                return new ServiceResource(CreateServiceResourceName(type, methodInfo), ServiceBaseResourceName(type), authorizeAttribute.Permissions, methodInfo, isPublic);
            }
            else
            {
                return new ServiceResource(CreateServiceResourceName(type, methodInfo), ServiceBaseResourceName(type), Permissions.All, methodInfo, isPublic);
            }
        }

        private static Resource GetDataResourceFromMethod(Type type, MethodInfo methodInfo)
        {
            var dataEntityAttribute = type
                .GetCustomAttributes(true)
                .Where(z => z is DataEntityAttribute)
                .Select(x => x as DataEntityAttribute)
                .FirstOrDefault();
            var authorizeAttribute = methodInfo.GetCustomAttribute<AuthorizeAttribute>();
            var isPublic = methodInfo.GetCustomAttribute<InternalAttribute>() == null;
            var dataResourceName = CreateDataResourceName(type, methodInfo);

            if (authorizeAttribute != null)
            {
                return new DataEntityResource(dataResourceName, DataBaseResourceName(type), authorizeAttribute.Permissions, methodInfo, dataEntityAttribute, isPublic);
            }
            else
            {
                return new DataEntityResource(dataResourceName, DataBaseResourceName(type), Permissions.All, methodInfo, dataEntityAttribute, isPublic);
            }
        }

        private static string CreateServiceResourceName(Type type, MethodInfo method)
        {
            return $"{ServiceBaseResourceName(type)}/{method.Name}";
        }

        private static string CreateDataResourceName(Type type, MethodInfo method)
        {
            return $"{DataBaseResourceName(type)}/{method.Name}";
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

        private static List<Resource> GetDataResourceFromCrud(Type type, bool includePrivate = false)
        {
            var dataEntityAttribute = type
                .GetCustomAttributes(true)
                .Where(z => z is DataEntityAttribute)
                .Select(x => x as DataEntityAttribute)
                .FirstOrDefault();

            var dataResources = new List<Resource>();

            if (dataEntityAttribute.DataEntityMethods.HasFlag(DataEntityMethods.Create)
                || dataEntityAttribute.DataEntityMethods.HasFlag(DataEntityMethods.Read)
                || dataEntityAttribute.DataEntityMethods.HasFlag(DataEntityMethods.Update)
                || dataEntityAttribute.DataEntityMethods.HasFlag(DataEntityMethods.Delete))
            {
                dataResources.Add(new DataEntityResource($"{dataEntityAttribute.Name}", DataBaseResourceName(type), Permissions.All, CreateMethodInfo(type, "Create"), dataEntityAttribute, true));
            }

            return dataResources;
        }

        static MethodInfo CreateMethodInfo(Type dataEntityType, string method)
        {
            return typeof(IDataEntity<>).MakeGenericType(dataEntityType).GetMethod(method);
        }
    }

}
