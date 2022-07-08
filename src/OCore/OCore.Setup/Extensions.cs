using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OCore.Diagnostics;
using OCore.Entities.Data.Http;
using OCore.Http.OpenApi;
using OCore.Services;
using OCore.Services.Http;

namespace OCore.DefaultSetup
{
    public static class Extensions
    {
        public static IServiceCollection AddDefaultOCore(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddServiceRouter()
                .AddDiagnosticIncomingGrainCallFilter();                ;
        }

        public static void UseDefaultOCore(this IApplicationBuilder app,
            string appTitle = "OCore app development",
            string version = "Development")
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapServices("services");
                endpoints.MapDataEntities("data");
                endpoints.MapDeveloperOpenApi("api-docs",
                    appTitle,
                    version);
            });
        }

    }
}
