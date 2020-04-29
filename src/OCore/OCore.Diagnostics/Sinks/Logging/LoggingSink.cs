using Microsoft.Extensions.Logging;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Diagnostics.Sinks.Logging
{
    public class LoggingSink : IDiagnosticsSink
    {
        public bool Enabled { get; set; } = true;

        ILogger logger;
        public LoggingSink(ILogger<LoggingSink> logger)
        {
            this.logger = logger;
        }

        public Task AddRequest(DiagnosticsPayload request, IGrainCallContext grainCallContext)
        {
            if (Enabled == false) return Task.CompletedTask;

            // Do not log internal Orleans calls
            
            if (grainCallContext.Grain.GetType().FullName.StartsWith("Orleans.")) return Task.CompletedTask;
            logger.LogInformation(request.ToString());

            return Task.CompletedTask;
        }

        public Task Complete(DiagnosticsPayload request, IGrainCallContext grainCallContext)
        {
            return Task.CompletedTask;
        }
    }
}
