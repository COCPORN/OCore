﻿using Microsoft.AspNetCore.Builder;
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

        public static Task LetsGo(Action<HostBuilder> hostConfigurationDelegate = null,
            Action<ISiloBuilder> siloConfigurationDelegate = null)
        {
            var hostBuilder = new HostBuilder();
            hostBuilder.DeveloperSetup(siloConfigurationDelegate);
            hostConfigurationDelegate?.Invoke(hostBuilder);
            var host = hostBuilder.Build();
            return host.StartAsync();
        }

        public static void DeveloperSetup(this HostBuilder hostBuilder, Action<ISiloBuilder> configureDelegate = null)
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
                b.ConfigureApplicationParts(parts => parts.AddFromApplicationBaseDirectory());

                configureDelegate?.Invoke(b);
            });
        }

    }
}
