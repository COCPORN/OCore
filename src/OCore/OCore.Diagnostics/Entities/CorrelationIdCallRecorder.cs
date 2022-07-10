using OCore.Diagnostics.Abstractions;
using OCore.Entities.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Diagnostics.Entities
{
    public class CorrelationIdCallRecorder : DataEntity<CorrelationIdCallRecord>, ICorrelationIdCallRecorder
    {
        public Task Complete(string methodName, string result)
        {
            throw new NotImplementedException();
        }

        public Task Fail(string message)
        {
            throw new NotImplementedException();
        }

        public Task Request(string methodName, string parameters)
        {
            throw new NotImplementedException();
        }

    }
}
