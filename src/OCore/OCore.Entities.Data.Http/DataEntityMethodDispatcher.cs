using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OCore.Http;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OCore.Entities.Data.Http
{
    public class DataEntityMethodDispatcher : DataEntityDispatcher
    {
        DataEntityGrainInvoker invoker;
        MethodInfo methodInfo;
        IClusterClient clusterClient;
        Type grainType;

        public DataEntityMethodDispatcher(
            IEndpointRouteBuilder routeBuilder,
            string prefix,
            string dataEntityName,
            KeyStrategy keyStrategy,
            Type grainType,           
            MethodInfo methodInfo) : 
            base(prefix, dataEntityName, keyStrategy)
        {
            this.grainType = grainType;
            this.methodInfo = methodInfo;
            clusterClient = routeBuilder.ServiceProvider.GetRequiredService<IClusterClient>();

            invoker = new DataEntityGrainInvoker(routeBuilder.ServiceProvider, grainType, methodInfo, null);
            routeBuilder.MapPost(GetRoutePattern(methodInfo.Name).RawText, Dispatch);
        }

        public async Task Dispatch(HttpContext httpContext)
        {
            httpContext.RunAuthorizationFilters(invoker);
            httpContext.RunActionFilters(invoker);

            var grainId = GetKey(httpContext);
            var grain = clusterClient.GetGrain(grainType, grainId.Key);
            if (grain == null)
            {
                httpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                return;
            }

            await invoker.Invoke(grain, httpContext);
        }

        public static DataEntityMethodDispatcher Register(IEndpointRouteBuilder routeBuilder, 
            string prefix, 
            string dataEntityName, 
            KeyStrategy keyStrategy,
            Type grainType,
            MethodInfo methodInfo)
        {
            return new DataEntityMethodDispatcher(routeBuilder, 
                prefix, 
                dataEntityName, 
                keyStrategy,
                grainType,
                methodInfo);
        }

    }
}
