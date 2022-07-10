using OCore.Diagnostics.Abstractions;
using OCore.Diagnostics.Filters;
using OCore.Entities.Data.Extensions;
using Orleans;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Diagnostics.Sinks.Graphing
{    
    public class MermaidSink : IDiagnosticsSink
    {
        public bool IsPaused { get; set; } = false;
        public bool EnableOCoreInternal { get; set; } = false;

        private readonly IGrainFactory grainFactory;

        public MermaidSink(IGrainFactory grainFactory)
        {
            this.grainFactory = grainFactory;
        }

        public async Task Complete(DiagnosticsPayload request, IGrainCallContext grainCallContext)
        {
            var recorderGrain = grainFactory.GetDataEntity<ICorrelationIdCallRecorder>(request.CorrelationId);

            var result = string.Empty; // Get result from grainCallContext

            await recorderGrain.Complete(request.MethodName, result);
        }

        public Task Fail(DiagnosticsPayload request, IGrainCallContext grainCallContext, Exception ex)
        {
            throw new NotImplementedException();
        }

        public Task Request(DiagnosticsPayload request, IGrainCallContext grainCallContext)
        {
            throw new NotImplementedException();
        }
    }
}
