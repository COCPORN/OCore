using OCore.Diagnostics.Filters;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Diagnostics.Sinks.Graphing
{
    public class GraphingSink : IDiagnosticsSink
    {
        public bool IsPaused { get; set; } = false;
        public bool EnableOCoreInternal { get; set; } = false;

        public Task Complete(DiagnosticsPayload request, IGrainCallContext grainCallContext)
        {
            throw new NotImplementedException();
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
