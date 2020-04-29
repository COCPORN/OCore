using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OCore.Diagnostics
{
    public class DiagnosticIncomingGrainCallFilter : IIncomingGrainCallFilter
    {
        IEnumerable<IDiagnosticsSink> sinks;

        public DiagnosticIncomingGrainCallFilter(IEnumerable<IDiagnosticsSink> sinks)
        {
            this.sinks = sinks;
        }

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            var payload = DiagnosticsPayload.GetOrDefault();
            if (payload == null)
            {
                payload = DiagnosticsPayload.Register(c => c.RequestSource = RequestSource.Filter);
            }

            // Update payload
            payload.HopCount++;
            payload.PreviousGrainName = payload.GrainName;
            payload.PreviousMethodName = payload.MethodName;
            payload.GrainName = $"{context?.Grain}";
            payload.MethodName = $"{context?.InterfaceMethod?.Name}";

            await Task.WhenAll(sinks.Select(s => s.AddRequest(payload, context)));
            try
            {
                await Task.WhenAll(context.Invoke());
                await Task.WhenAll(sinks.Select(s => s.Complete(payload, context)));
            }
            catch (Exception ex)
            {
                await Task.WhenAll(sinks.Select(s => s.Fail(payload, context, ex)));
                throw;
            }

        }
    }
}
