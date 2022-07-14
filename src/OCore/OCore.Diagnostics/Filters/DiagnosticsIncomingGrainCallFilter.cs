using Microsoft.Extensions.Logging;
using OCore.Core.Extensions;
using OCore.Diagnostics.Filters;
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
        readonly IEnumerable<IDiagnosticsSink> sinks;
        readonly ILogger logger;

        public DiagnosticIncomingGrainCallFilter(IEnumerable<IDiagnosticsSink> sinks,
            ILogger<DiagnosticIncomingGrainCallFilter> logger)
        {
            this.sinks = sinks;
            this.logger = logger;
        }

        // This was a super-bad abstraction and it didn't carry where it was supposed to,
        // it should be removed
        T? GetRequestContextValue<T>(string key, Func<T?, T>? update = null) where T : class
        {
            T? currentRequestContextValue = RequestContext.Get(key) as T;
            T? newRequestContextValue = currentRequestContextValue;

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
            var grainName = context.Grain.GetType().FullName ?? throw new NullReferenceException("Unable to get grain name");
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

            var contextTask = context.Invoke();

            Task.WhenAll(sinks.Select(s => s.Request(payload, context))).FireAndForget(logger);

            try
            {
                await contextTask;
            }
            catch (Exception ex)
            {
                Task.WhenAll(sinks.Select(s => s.Fail(payload, context, ex))).FireAndForget(logger);
                throw;
            }

            Task.WhenAll(sinks.Select(s => s.Complete(payload, context))).FireAndForget(logger);
        }
    }
}
