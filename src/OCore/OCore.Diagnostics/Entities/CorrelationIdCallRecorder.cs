using OCore.Diagnostics.Abstractions;
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

        [Id(1)]
        public string PreviousEntry { get; set; }

        [Id(2)]
        public string RequestSource { get; set; }
    }

    public class CorrelationIdCallRecorder : DataEntity<CorrelationIdCallRecord>, ICorrelationIdCallRecorder
    {
        public async Task Complete(string methodName, string result)
        {
            State.Entries.Add(new CallEntry
            {
                From = State.PreviousEntry,
                To = methodName,
                Result = result
            });

            State.PreviousEntry = methodName;

            await WriteStateAsync();
        }

        public async Task Fail(string methodName, string message)
        {
            State.Entries.Add(new CallEntry
            {
                From = methodName,
                ExceptionMessage = message
            });

            State.PreviousEntry = methodName;

            await WriteStateAsync();
        }

        public async Task Request(string methodName, string parameters)
        {

            State.Entries.Add(new CallEntry
            {
                From = State.PreviousEntry,
                To = methodName,
                Parameters = parameters
            });

            State.PreviousEntry = methodName;

            if (State.RequestSource == null)
            {
                State.RequestSource = RequestContext.Get("D:RequestSource") as string;
            }

            await WriteStateAsync();
        }

    }
}
