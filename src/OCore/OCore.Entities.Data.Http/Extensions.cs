using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OCore.Entities.Data.Http
{
    public static class Extensions
    {

        // TODO: Remove
        public static IServiceCollection AddDataEntityRouter(this IServiceCollection services)
        {
            return services;
        }
    }
}
