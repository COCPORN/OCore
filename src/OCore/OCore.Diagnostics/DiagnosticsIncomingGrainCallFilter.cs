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
            // I added this to make it possible to debug the filter because there are so many 
            // Orleans-specific messages running in the silo
#if DEBUG
            var grainName = context.Grain.GetType().FullName;

            if (grainName.StartsWith("Orleans")
                || grainName.StartsWith("OCore")
                || sinks.Count() == 0)
            {
                await context.Invoke();
                return;
            }
#endif

            if (sinks.Count() == 0)
            {
                await context.Invoke();
                return;
            }

            var payload = DiagnosticsPayload.GetOrDefault();

            if (payload == null)
            {
                payload = DiagnosticsPayload.Register(p =>
                {
                    p.RequestSource = RequestSource.Filter;
                    p.GrainName = $"{context?.Grain}";
                    p.MethodName = $"{context?.InterfaceMethod?.Name}";
                });
            }
            else
            {
                payload = DiagnosticsPayload.Register(p =>
                {
                    p.HopCount = payload.HopCount++;
                    p.PreviousGrainName = payload.GrainName;
                    p.PreviousMethodName = payload.MethodName;
                    p.GrainName = $"{context?.Grain}";
                    p.MethodName = $"{context?.InterfaceMethod?.Name}";
                    p.CreatedAt = payload.CreatedAt;
                    p.RequestSource = payload.RequestSource;
                    p.CorrelationId = payload.CorrelationId;                    
                });
            }

            await Task.WhenAll(sinks.Select(s => s.Request(payload, context)));
            try
            {
                await context.Invoke();
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
