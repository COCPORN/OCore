using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OCore.Entities.Data.Http;
using OCore.Services;
using OCore.Services.Http;
using System;

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
                endpoints.MapServices("service");
                endpoints.MapDataEntities("data");
            });
        }
    }
}
