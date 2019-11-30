using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OCore.Entities.Data.Http;
using OCore.Services;
using OCore.Services.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Orleans;
using Orleans.Hosting;
using OCore.Authorization;

namespace OCore.DefaultSetup
{
    public static class Extensions
    {
        public static IServiceCollection AddDefaultOCore(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddServiceRouter()
                .AddDataEntityRouter();
        }

        public static void UseDefaultOCore(this IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapServices("services");
                endpoints.MapDataEntities("data");
            });
        }

    }
}
