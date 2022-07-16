using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OCore.Diagnostics.Sinks.CorrelationId;
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
        public static void AddDiagnosticsSink<T>(this ISiloBuilder siloBuilder) where T : class, IDiagnosticsSink
        {
            siloBuilder.ConfigureServices(
                services =>
                {
                    AddDiagnosticIncomingGrainCallFilter(services);
                    services.AddSingleton<IDiagnosticsSink, T>();
                });
        }

        public static IServiceCollection AddDiagnosticIncomingGrainCallFilter(this IServiceCollection services)
        {
            services.TryAddSingleton<IIncomingGrainCallFilter, DiagnosticIncomingGrainCallFilter>();
            return services;
        }

        public static void AddLoggingDiagnostics(this ISiloBuilder siloBuilder)
        {
            siloBuilder.AddDiagnosticsSink<LoggingSink>();
        }

        public static void AddGraphingDiagnostics(this ISiloBuilder siloBuilder)
        {
            siloBuilder.AddDiagnosticsSink<CorrelationIdRecordingSink>();
        }

        public static void AddOCoreDeveloperDiagnostics(this ISiloBuilder siloBuilder)
        {
            siloBuilder.AddLoggingDiagnostics();
            siloBuilder.AddGraphingDiagnostics();
        }
    }
}
