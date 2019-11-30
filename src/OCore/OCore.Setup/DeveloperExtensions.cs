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
using System;
using System.Threading.Tasks;

namespace OCore.Setup
{
    public static class DeveloperExtensions
    {
        public static Task LetsGo(Type assemblyCarrier = null)
        {
            var hostBuilder = new HostBuilder();
            return hostBuilder.LetsGo(assemblyCarrier);
        }

        public static Task LetsGo(Action<HostBuilder> configure, Type assemblyCarrier = null)
        {
            var hostBuilder = new HostBuilder();
            hostBuilder.DeveloperSetup(assemblyCarrier);
            configure(hostBuilder);
            var host = hostBuilder.Build();
            return host.StartAsync();
        }

        public static Task LetsGo(this HostBuilder hostBuilder, Type grainType = null)
        {
            hostBuilder.DeveloperSetup(grainType);
            var host = hostBuilder.Build();
            return host.StartAsync();
        }

        public static void DeveloperSetup(this HostBuilder hostBuilder, Type grainType = null)
        {
            hostBuilder.UseConsoleLifetime();
            hostBuilder.ConfigureLogging(logging => logging.AddConsole());
            hostBuilder.ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseUrls("http://*:9000");
                webBuilder.UseStartup<DeveloperStartup>();
            });

            hostBuilder.UseOrleans(b =>
            {
                b.UseLocalhostClustering();
                b.AddMemoryGrainStorage("PubSubStore");
                b.AddSimpleMessageStreamProvider("BaseStreamProvider");
                b.AddMemoryGrainStorageAsDefault();
                b.AddOCoreAuthorization();
                if (grainType != null)
                {
                    b.ConfigureApplicationParts(parts => parts
                        .AddApplicationPart(grainType.Assembly)
                        .WithReferences());
                }
                else
                {
                    b.ConfigureApplicationParts(parts => parts.AddFromApplicationBaseDirectory());
                }
            });
        }

    }
}
