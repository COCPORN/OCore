using Orleans;
using System.Threading.Tasks;

namespace OCore.Diagnostics
{
    /// <summary>
    /// Implement this to have the diagnostic system deliver
    /// diagnostic messages to you
    /// </summary>
    public interface IDiagnosticsSink
    {
        bool Enabled { get; set; }

        Task AddRequest(DiagnosticsPayload request, IGrainCallContext grainCallContext);

        Task Complete(DiagnosticsPayload request, IGrainCallContext grainCallContext);
    }
}