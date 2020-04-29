using Microsoft.Extensions.DependencyInjection;
using OCore.Diagnostics.Sinks.Logging;
using Orleans;
using Orleans.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace OCore.Diagnostics
{
    public static class Extensions
    {
        static bool addedIncomingGrainCallFilter = false;
        static object mutex = new object();

        public static void AddSink<T>(this ISiloBuilder siloBuilder) where T : class, IDiagnosticsSink            
        {
            siloBuilder.ConfigureServices(
                services =>
                {
                    AddDiagnosticIncomingGrainCallFilter(services);                    
                    services.AddSingleton<IDiagnosticsSink, T>();
                });
        }

        public static void AddDiagnosticIncomingGrainCallFilter(IServiceCollection services)
        {
            // To be honest, I don't know if there is a more elegant way of doing this. I HAVE NO TIME! TO LEARN!
            lock (mutex)
            {
                if (addedIncomingGrainCallFilter == false)
                {
                    services.AddSingleton<IIncomingGrainCallFilter, DiagnosticIncomingGrainCallFilter>();
                    addedIncomingGrainCallFilter = true;
                }
            }
        }

        public static void AddLoggingDiagnostics(this ISiloBuilder siloBuilder)
        {
            siloBuilder.AddSink<LoggingSink>();
        }

        public static void AddOCoreDeveloperDiagnostics(this ISiloBuilder siloBuilder)
        {
            siloBuilder.AddLoggingDiagnostics();
        }
    }
}
