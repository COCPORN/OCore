using Microsoft.Extensions.Options;
using OCore.Diagnostics;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Dashboard.Diagnostics.Sinks
{
    public class DashboardSink : IDiagnosticsSink
    {
        DashboardOptions options;

        public DashboardSink(IOptions<DashboardOptions> options)
        {
            this.options = options.Value;
        }

        public bool IsPaused { get; set; }
        public bool EnableOCoreInternal { get; set; }

        public Task Complete(DiagnosticsPayload request, IGrainCallContext grainCallContext)
        {
            return Task.CompletedTask;
        }

        public Task Fail(DiagnosticsPayload request, IGrainCallContext grainCallContext, Exception ex)
        {
            return Task.CompletedTask;
        }

        public Task Request(DiagnosticsPayload request, IGrainCallContext grainCallContext)
        {
            return Task.CompletedTask;
        }
    }
}
