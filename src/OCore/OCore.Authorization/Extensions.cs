using Orleans;
using Orleans.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace OCore.Authorization
{
    public static class Extensions
    {
        public static void AddOCoreAuthorization(this ISiloBuilder siloBuilder)
        {
            siloBuilder
                .ConfigureApplicationParts(apm => apm.AddApplicationPart(typeof(Services.AuthorizationService).Assembly)
                .WithReferences());
        }
    }
}
