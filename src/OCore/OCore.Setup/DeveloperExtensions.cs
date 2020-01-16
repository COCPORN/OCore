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
using Microsoft.Extensions.Configuration;

namespace OCore.Setup
{
    public static class DeveloperExtensions
    {

        public static Task LetsGo(Action<HostBuilder> hostConfigurationDelegate = null,
            Action<ISiloBuilder> siloConfigurationDelegate = null,
            Action<Microsoft.Extensions.Hosting.HostBuilderContext, IServiceCollection> serviceConfigurationDelegate = null)
        {
            var hostBuilder = new HostBuilder();
            hostBuilder.DeveloperSetup(siloConfigurationDelegate);
            hostBuilder.ConfigureServices(serviceConfigurationDelegate);
            hostConfigurationDelegate?.Invoke(hostBuilder);
            var host = hostBuilder.Build();
            return host.StartAsync();
        }

        public static void DeveloperSetup(this HostBuilder hostBuilder,
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

            hostBuilder.UseOrleans(b =>
            {
                b.UseLocalhostClustering();
                b.AddMemoryGrainStorage("PubSubStore");
                b.AddSimpleMessageStreamProvider("BaseStreamProvider");
                b.AddMemoryGrainStorageAsDefault();
                b.AddOCoreAuthorization();
                b.ConfigureApplicationParts(parts => parts.AddFromApplicationBaseDirectory());

                siloConfigurationDelegate?.Invoke(b);
            });
        }

    }
}
