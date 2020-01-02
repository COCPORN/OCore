using OCore.Authorization.Abstractions;
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
        public string ResourceName { get; private set; }
        public Permissions Permissions { get; private set; }

        public Resource(string resourceName, Permissions permission)
        {
            ResourceName = resourceName;
            Permissions = permission;
        }
    }


    public class ServiceResource : Resource
    {
        public ServiceResource(string resourceName, Permissions permission) : base(resourceName, permission)
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
                    resources = FindAllGrainInterfacesAndMethods();
                }
                return resources;
            }
        }

        static List<Resource> FindAllGrainInterfacesAndMethods()
        {
            var serviceInterfaces = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => x.IsSubclassOf(typeof(Service)))                
                .SelectMany(x => x.GetInterfaces())
                .Where(x => x.GetCustomAttributes(true).Select(z => z is ServiceAttribute).Any());                
            return serviceInterfaces
                .SelectMany(x => x.GetMethods()
                               .SelectMany(y => y.GetCustomAttributes(true)
                                                 .Where(z => z is AuthorizeAttribute)
                                                 .Select(z =>
                                                 {
                                                     var aa = (AuthorizeAttribute)z;
                                                     return new ServiceResource(CreateServiceResourceName(x, y), aa.Permissions) as Resource;
                                                 }))).ToList();
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
    }

}
