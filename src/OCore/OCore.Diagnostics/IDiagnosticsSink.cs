using OCore.Diagnostics.Filters;
using Orleans;
using System;
using System.Threading.Tasks;

namespace OCore.Diagnostics
{
    /// <summary>
    /// Implement this to have the diagnostic system deliver
    /// diagnostic messages to you. This sink lives in the context
    /// of the grain filters
    /// </summary>
    public interface IDiagnosticsSink
    {
        bool IsPaused { get; set; }

        bool EnableOCoreInternal { get; set;  }

        Task Request(DiagnosticsPayload request, IGrainCallContext grainCallContext);

        Task Complete(DiagnosticsPayload request, IGrainCallContext grainCallContext);

        Task Fail(DiagnosticsPayload request, IGrainCallContext grainCallContext, Exception ex);
    }
}