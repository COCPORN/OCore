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
        Task Request(string methodName, string parameters);

        [Internal]
        Task Complete(string methodName, string result);

        [Internal]
        Task Fail(string methodName, string message);
    }
}
