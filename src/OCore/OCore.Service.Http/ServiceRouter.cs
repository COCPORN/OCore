using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OCore.Service
{
    public class ServiceRouter
    {
        public void RegisterRoute(string pattern, MethodInfo method)
        {
            CheckGrainType(method.DeclaringType);
        }

        private void CheckGrainType(Type grainInterfaceType)
        {
            var ifaces = grainInterfaceType.GetInterfaces();
            if (ifaces.Contains(typeof(IGrainWithIntegerKey)) == false)
            {
                throw new InvalidOperationException("Service is not of correct type");
            }

        }

        public Task Dispatch(HttpContext context)
        {
            var endpoint = (RouteEndpoint)context.GetEndpoint();
            var pattern = endpoint.RoutePattern;
            return Task.CompletedTask;
        }
    }
}
