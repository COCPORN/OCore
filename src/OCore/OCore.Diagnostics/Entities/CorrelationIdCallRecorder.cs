using Microsoft.Extensions.Options;
using OCore.Diagnostics.Abstractions;
using OCore.Diagnostics.Options;
using OCore.Entities.Data;
using Orleans;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Diagnostics.Entities
{
    [Serializable]
    [GenerateSerializer]
    public class CallEntry
    {
        [Id(0)]
        public string From { get; init; }

        [Id(1)]
        public string To { get; init; }

        [Id(2)]
        public string Parameters { get; init; }

        [Id(3)]
        public string Result { get; init; }

        [Id(4)]
        public string ExceptionMessage { get; init; }
    }

    [Serializable]
    [GenerateSerializer]
    public class CorrelationIdCallRecord
    {

        [Id(0)]
        public List<CallEntry> Entries { get; set; } = new List<CallEntry>();

        [Id(2)]
        public string RequestSource { get; set; }
    }

    public class CorrelationIdCallRecorder : DataEntity<CorrelationIdCallRecord>, ICorrelationIdCallRecorder
    {
        readonly DiagnosticsOptions diagnosticsOptions;

        public CorrelationIdCallRecorder(IOptions<DiagnosticsOptions> diagnosticsOptions)
        {
            this.diagnosticsOptions = diagnosticsOptions.Value;
        }

        public async Task Complete(string from, string to, string result)
        {
            State.Entries.Add(new CallEntry
            {
                From = from,
                To = to,
                Result = result
            });

            if (diagnosticsOptions.StoreCorrelationIdData == true)
            {
                await WriteStateAsync();
            }
        }

        public async Task Fail(string methodName, string message)
        {
            State.Entries.Add(new CallEntry
            {
                From = methodName,
                ExceptionMessage = message
            });

            if (diagnosticsOptions.StoreCorrelationIdData == true)
            {
                await WriteStateAsync();
            }
        }

        public async Task Request(string from, string to, string parameters)
        {
            State.Entries.Add(new CallEntry
            {
                To = to,
                From = from,
                Parameters = parameters
            });

            if (State.RequestSource == null)
            {
                State.RequestSource = RequestContext.Get("D:RequestSource") as string;
            }

            if (diagnosticsOptions.StoreCorrelationIdData == true)
            {
                await WriteStateAsync();
            }
        }

        public Task<string> ToMermaid()
        {
            var sb = new StringBuilder();
            var participants = new HashSet<string>();

            if (State.RequestSource != null)
            {
                participants.Add(State.RequestSource);
            }

            foreach (var entry in State.Entries)
            {
                if (entry.From != null)
                {
                    participants.Add(entry.From);
                }

                if (entry.To != null)
                {
                    participants.Add(entry.To);
                }
            }

            sb.AppendLine("sequenceDiagram");

            foreach (var participant in participants)
            {
                sb.AppendLine($"   participant {participant}");
            }

            foreach (var entry in State.Entries)
            {
                var from = entry.From;
                if (from == null)
                {
                    from = State.RequestSource;
                }

                var to = entry.To;
                if (to == null)
                {
                    to = State.RequestSource;
                }

                if (entry.Parameters != null)
                {
                    sb.AppendLine($"   {from}->>+{to}: {entry.Parameters}");
                }

                if (entry.Result != null)
                {
                    sb.AppendLine($"   {from}->>-{to}: {entry.Result}");
                }

                if (entry.ExceptionMessage != null)
                {
                    sb.AppendLine($"   {from}-x-{to}: {entry.ExceptionMessage}");
                }
            }

            return Task.FromResult(sb.ToString());
        }
    }
}
