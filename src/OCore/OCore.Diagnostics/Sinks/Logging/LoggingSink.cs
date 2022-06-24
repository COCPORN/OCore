using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OCore.Core.Extensions;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Diagnostics.Sinks.Logging
{
    public class LoggingSinkOptions
    {
        public bool Enabled { get; set; } = true;

        public bool LogArguments { get; set; } = true;
    }

    public class LoggingSink : IDiagnosticsSink
    {
        public bool IsPaused { get; set; } = true;

        public bool EnableOCoreInternal { get; set; } = false;

        ILogger logger;
        LoggingSinkOptions options;
        public LoggingSink(ILogger<LoggingSink> logger,
            IOptions<LoggingSinkOptions> options)
        {
            this.logger = logger;
            this.options = options.Value;
        }

        public Task Request(DiagnosticsPayload request, IGrainCallContext grainCallContext)
        {
            if (CheckWhetherToLog(grainCallContext) == false) return Task.CompletedTask;
            // Todo FIX THIS
            logger.LogInformation($"[{grainCallContext.Grain.ToString()}] {request}");

            return Task.CompletedTask;
        }

        private bool CheckWhetherToLog(IGrainCallContext grainCallContext)
        {
            if (IsPaused == true || options.Enabled == false) return false;

            var fullName = grainCallContext.Grain.GetType().FullName; ;

            // Do not log internal calls
            if (fullName.StartsWith("Orleans.")) return false;
            if (EnableOCoreInternal == false && fullName.StartsWith("OCore.")) return false;
            return true;
        }

        public Task Complete(DiagnosticsPayload request, IGrainCallContext grainCallContext)
        {
            if (CheckWhetherToLog(grainCallContext) == false) return Task.CompletedTask;
            if (options.LogArguments == true)
            {
                logger.LogInformation($"> {JsonConvert.SerializeObject(grainCallContext.Arguments)}\n" + 
                    $"< {JsonConvert.SerializeObject(grainCallContext.Result)}");                
            }
            return Task.CompletedTask;
        }

        public Task Fail(DiagnosticsPayload request, IGrainCallContext grainCallContext, Exception ex)
        {
            if (CheckWhetherToLog(grainCallContext) == false) return Task.CompletedTask;
            if (options.LogArguments == true)
            {
                logger.LogError(ex, $"> {JsonConvert.SerializeObject(grainCallContext.Arguments)}");
            }
            return Task.CompletedTask;            
        }
    }
}
