using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OCore.Authorization.Abstractions;
using OCore.Authorization.Abstractions.Request;
using OCore.Http;
using Orleans;
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace OCore.Entities.Data.Http
{
    public class DataEntityCrudDispatcher : DataEntityDispatcher
    {
        DataEntityGrainInvoker invoker;
        IClusterClient clusterClient;
        IPayloadCompleter payloadCompleter;
        Type grainType;
        HttpMethod httpMethod;

        public DataEntityCrudDispatcher(IEndpointRouteBuilder routeBuilder,
            string prefix,
            string dataEntityName,
            KeyStrategy keyStrategy,
            int maxFanoutLimit,
            Type grainType,
            Type dataEntityType,
            IPayloadCompleter payloadCompleter,
            HttpMethod httpMethod) : base(prefix, dataEntityName, keyStrategy, maxFanoutLimit)
        {
            this.grainType = grainType;
            MethodInfo methodInfo = null;
            this.httpMethod = httpMethod;
            this.payloadCompleter = payloadCompleter;
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
                    methodInfo = typeof(IDataEntity<>).MakeGenericType(dataEntityType).GetMethod("Update");
                    break;
            }
            invoker = new DataEntityGrainInvoker(routeBuilder.ServiceProvider, grainType, methodInfo, dataEntityType)
            {
                IsCrudOperation = true,
                HttpMethod = httpMethod,
            };
            clusterClient = routeBuilder.ServiceProvider.GetRequiredService<IClusterClient>();
        }

        public async Task Dispatch(HttpContext httpContext)
        {
            try
            {
                httpContext.RunAuthorizationFilters(invoker);
                httpContext.RunActionFiltersExecuting(invoker);

                await httpContext.RunAsyncActionFilters(invoker, async (context) =>
                {
                    var payload = Payload.GetOrDefault();
                    if (payload != null)
                    {
                        await payloadCompleter.Complete(payload, clusterClient);
                    }

                    var grainKeys = GetKeys(httpContext);
                    if (grainKeys == null || grainKeys.Length == 0)
                    {
                        await httpContext.SetStatusCode(HttpStatusCode.BadRequest, "Unreachable destination");
                        return;
                    }

                    // Go get that single grain
                    if (grainKeys.Length == 1)
                    {
                        var grain = clusterClient.GetGrain(grainType, grainKeys[0]);
                        if (grain == null)
                        {
                            await httpContext.SetStatusCode(HttpStatusCode.BadRequest, "Unreachable destination");
                            return;
                        }

                        try
                        {
                            await invoker.Invoke(grain, httpContext);
                            httpContext.RunActionFiltersExecuted(invoker);
                        }
                        catch (DataCreationException ex)
                        {
                            switch (httpMethod)
                            {
                                case HttpMethod.Get:
                                case HttpMethod.Delete:
                                case HttpMethod.Put:
                                    throw new StatusCodeException(HttpStatusCode.NotFound, ex.Message, ex);
                                default:
                                    throw;
                            }
                        }
                    }
                    else
                    {
                        // Do the fan-out
                        if (httpMethod == HttpMethod.Get)
                        {
                            var grains = grainKeys.Select(x => clusterClient.GetGrain(grainType, x)).ToArray();
                            await invoker.Invoke(grains, context);
                            httpContext.RunActionFiltersExecuted(invoker);
                        }
                        else
                        {
                            throw new NotImplementedException("Fan-out is not (yet) implemented for this operation");
                        }
                    }
                });
            }
            catch (StatusCodeException ex)
            {
                await httpContext.SetStatusCode(ex.StatusCode, ex.Message);
                httpContext.Response.Headers.Clear();
            }
        }

        public static DataEntityCrudDispatcher Register(IEndpointRouteBuilder routeBuilder,
            string prefix,
            string dataEntityName,
            KeyStrategy keyStrategy,
            int maxFanoutLimit,
            Type grainType,
            Type dataEntityType,
            IPayloadCompleter payloadCompleter,
            HttpMethod httpMethod)
        {
            return new DataEntityCrudDispatcher(routeBuilder,
                prefix,
                dataEntityName,
                keyStrategy,
                maxFanoutLimit,
                grainType,
                dataEntityType,
                payloadCompleter,
                httpMethod);
        }

    }
}
