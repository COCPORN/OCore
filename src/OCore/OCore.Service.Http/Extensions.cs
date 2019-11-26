using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OCore.Services.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OCore.Services
{
    public static class Extensions
    {
        public static IServiceCollection AddDefaultOCore(this IServiceCollection serviceCollection)
        {
            return AddServiceRouter(serviceCollection);
        }

        public static IServiceCollection AddServiceRouter(this IServiceCollection services)
        {
            return services
                .AddSingleton<ServiceRouter>();
        }

        public static void UseDefaultOCore(this IApplicationBuilder app)
        {            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapServices("service");
            });
        }
    }
}
