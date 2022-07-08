using Orleans;
using Orleans.Runtime;
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

        T GetRequestContextValue<T>(string key, Func<T, T> update = null) where T : class
        {
            T currentRequestContextValue = RequestContext.Get(key) as T;
            T newRequestContextValue = currentRequestContextValue;

            if (update != null)
            {
                newRequestContextValue = update(currentRequestContextValue);
            }

            if (currentRequestContextValue != newRequestContextValue)
            {
                RequestContext.Set(key, newRequestContextValue);
                return newRequestContextValue;
            }
            else
            {
                return currentRequestContextValue;
            }
        }

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            // I added this to make it possible to debug the filter because there are so many 
            // Orleans-specific messages running in the silo
            var grainName = context.Grain.GetType().FullName;
#if DEBUG

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

            var correlationId = GetRequestContextValue<string>("D:CorrelationId", cid => cid == null ? Guid.NewGuid().ToString() : cid);
            var requestSource = GetRequestContextValue<string>("D:RequestSource", rs => rs == null ? RequestSource.Filter.ToString() : rs);
            var previousGrainName = RequestContext.Get("D:GrainName") as string;
            var previousMethodName = RequestContext.Get("D:MethodName") as string;

            if (RequestContext.Get("D:HopCount") is int hopCount == false)
            {
                hopCount = 0;
            }
            else
            {
                hopCount = hopCount + 1;
            }
            RequestContext.Set("D:HopCount", hopCount);

            if (RequestContext.Get("D:CreatedAt") is DateTimeOffset createdTime == false)
            {
                createdTime = DateTimeOffset.UtcNow;
                RequestContext.Set("D:CreatedAt", createdTime);
            }

            var methodName = context.InterfaceMethod.Name;

            RequestContext.Set("D:GrainName", grainName);
            RequestContext.Set("D:MethodName", methodName);

            var payload = new DiagnosticsPayload
            {
                CorrelationId = correlationId,
                CreatedAt = createdTime,
                HopCount = hopCount,
                PreviousGrainName = previousGrainName,
                PreviousMethodName = previousMethodName,
                RequestSource = requestSource,
                GrainName = grainName,
                MethodName = methodName,
            };

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
