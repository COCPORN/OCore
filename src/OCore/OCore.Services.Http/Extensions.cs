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


        public static IServiceCollection AddServiceRouter(this IServiceCollection services)
        {
            return services
                .AddSingleton<ServiceRouter>();
        }


    }
}
