using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using OCore.Http;
using Orleans;
using Microsoft.Extensions.DependencyInjection;

namespace OCore.Entities.Data.Http
{
    public class DataEntityCrudDispatcher : DataEntityDispatcher
    {        
        DataEntityGrainInvoker invoker;
        IClusterClient clusterClient;
        Type interfaceType;

        public DataEntityCrudDispatcher(IEndpointRouteBuilder routeBuilder, 
            string prefix,
            string dataEntityName,
            KeyStrategy keyStrategy,
            Type interfaceType,
            Type dataEntityType,
            HttpMethod httpMethod) : base(prefix, dataEntityName, keyStrategy)
        {
            this.interfaceType = interfaceType;
            MethodInfo methodInfo = null;
            switch (httpMethod)
            {          
                case HttpMethod.Post:
                    routeBuilder.MapPost(GetRoutePattern().RawText, Dispatch);
                    methodInfo = typeof(IDataEntity<>).MakeGenericType(dataEntityType).GetMethod("Create");
                    break;
                case HttpMethod.Get:
                    routeBuilder.MapGet(GetRoutePattern().RawText, Dispatch);
                    methodInfo = typeof(IDataEntity<>).MakeGenericType(dataEntityType).GetMethod("Read");
                    break;
                case HttpMethod.Delete:
                    routeBuilder.MapDelete(GetRoutePattern().RawText, Dispatch);
                    methodInfo = typeof(IDataEntity<>).MakeGenericType(dataEntityType).GetMethod("Delete");
                    break;
                case HttpMethod.Put:
                    routeBuilder.MapPut(GetRoutePattern().RawText, Dispatch);
                    methodInfo = typeof(IDataEntity<>).MakeGenericType(dataEntityType).GetMethod("Upsert");
                    break;
            }
            invoker = new DataEntityGrainInvoker(routeBuilder.ServiceProvider, methodInfo.DeclaringType, methodInfo, dataEntityType)
                        {
                            IsCrudOperation = true,
                            HttpMethod = httpMethod,
                        };
            clusterClient = routeBuilder.ServiceProvider.GetRequiredService<IClusterClient>();
        }

        public async Task Dispatch(HttpContext httpContext)
        {
            httpContext.RunAuthorizationFilters(invoker);
            httpContext.RunActionFilters(invoker);

            var grainId = GetKey(httpContext);
            var grain = clusterClient.GetGrain(interfaceType, grainId.Key);
            if (grain == null)
            {
                httpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                return;
            }

            await invoker.Invoke(grain, httpContext);
        }

        public static DataEntityCrudDispatcher Register(IEndpointRouteBuilder routeBuilder,
            string prefix,
            string dataEntityName,
            KeyStrategy keyStrategy,
            Type interfaceType,
            Type dataEntityType,
            HttpMethod httpMethod)
        {
            return new DataEntityCrudDispatcher(routeBuilder, 
                prefix,
                dataEntityName, 
                keyStrategy, 
                interfaceType,
                dataEntityType, 
                httpMethod);
        }

    }
}
