using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OCore.Authorization;
using OCore.Diagnostics;
using Orleans.Hosting;
using Orleans.Providers;
using System;
using System.Threading.Tasks;

namespace OCore.Setup
{
    public static class DeveloperExtensions
    {
        public static async Task LetsGo(Action<IHostBuilder> hostConfigurationDelegate = null,
            Action<ISiloBuilder> siloConfigurationDelegate = null,
            Action<HostBuilderContext, IServiceCollection> serviceConfigurationDelegate = null)
        {
            var hostBuilder = new HostBuilder();
            hostBuilder.DeveloperSetup(siloConfigurationDelegate);
            if (serviceConfigurationDelegate != null)
            {
                hostBuilder.ConfigureServices(serviceConfigurationDelegate);
            }
            hostConfigurationDelegate?.Invoke(hostBuilder);
            var host = hostBuilder.Build();
            await host.StartAsync();
            Console.ReadLine();
        }

        public static void DeveloperSetup(this IHostBuilder hostBuilder,
            Action<ISiloBuilder> siloConfigurationDelegate = null)
        {
            var configuration = new ConfigurationBuilder()
                 .AddEnvironmentVariables()
                 .AddJsonFile("appsettings.json", optional: true)
                 .Build();

            hostBuilder.UseConsoleLifetime();

            hostBuilder.ConfigureLogging(logging => logging.AddConsole());

            hostBuilder.ConfigureAppConfiguration(builder =>
            {
                builder.AddConfiguration(configuration);
            });

            hostBuilder.ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseUrls("http://*:9000");
                webBuilder.UseStartup<DeveloperStartup>();
            });            

            hostBuilder.UseOrleans((hostBuilderContext, siloBuilder) =>
            {
                siloBuilder.UseLocalhostClustering();
                siloBuilder.AddMemoryGrainStorage("PubSubStore");                
                siloBuilder.AddMemoryStreams<DefaultMemoryMessageBodySerializer>("MemoryStreamProvider");
                siloBuilder.AddMemoryGrainStorageAsDefault();
                siloBuilder.AddOCoreAuthorization();
                siloBuilder.AddOCoreDeveloperDiagnostics();
                siloBuilder.ConfigureLogging(builder => builder.SetMinimumLevel(LogLevel.Information));
                siloConfigurationDelegate?.Invoke(siloBuilder);
            });
        }

    }
}
