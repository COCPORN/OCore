using Microsoft.Extensions.DependencyInjection;
using OCore.Authorization.Abstractions;
using OCore.Authorization.Filters;
using Orleans;
using Orleans.Hosting;

namespace OCore.Authorization
{
    public static class Extensions
    {
        public static void AddOCoreAuthorization(this ISiloBuilder siloBuilder)
        {
            siloBuilder
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IIncomingGrainCallFilter, AuthorizationFilter>();
                    services.AddSingleton<IPayloadCompleter, PayloadCompleter>();
                });
        }
    }
}
