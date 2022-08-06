using OCore.Core;
using OCore.Diagnostics.Entities;
using OCore.Entities.Data;
using System.Threading.Tasks;

namespace OCore.Diagnostics.Abstractions
{
    [DataEntity("CorrelationIdRecorder", dataEntityMethods: DataEntityMethods.Read)]
    public interface ICorrelationIdCallRecorder : IDataEntity<CorrelationIdCallRecord>
    {
        [Internal]
        Task Request(string? from, string to, string parameters);

        [Internal]
        Task Complete(string? from, string to, string result);

        [Internal]
        Task Fail(string methodName, string exceptionType, string message);

        Task<string> ToMermaid();
    }
}
