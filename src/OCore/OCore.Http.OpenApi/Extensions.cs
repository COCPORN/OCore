using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace OCore.Http.OpenApi
{
    public static class Extensions
    {
        public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services)
        {
            return services;
        }

        public static IEndpointRouteBuilder MapOpenApi(this IEndpointRouteBuilder routes, string prefix = "")
        {
            return routes;
        }
    }
}
