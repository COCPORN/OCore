using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
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

        public static IEndpointRouteBuilder MapOpenApi(this IEndpointRouteBuilder routes, string prefix, string appTitle)
        {
            var routePattern = RoutePatternFactory.Parse($"{prefix}");

            var handler = new OpenApiHandler(appTitle);

            routes.MapGet(routePattern.RawText, handler.Dispatch);
            return routes;
        }
    }
}
