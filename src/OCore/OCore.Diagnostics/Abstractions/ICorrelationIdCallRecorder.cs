using OCore.Diagnostics.Entities;
using OCore.Entities.Data;
using System.Threading.Tasks;

namespace OCore.Diagnostics.Abstractions
{
    public interface ICorrelationIdCallRecorder : IDataEntity<CorrelationIdCallRecord>
    {
        Task Request(string methodName, string parameters);

        Task Complete(string methodName, string result);

        Task Fail(string message);
    }
}
