using Microsoft.AspNetCore.Http;
using OCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OCore.Entities.Data.Http
{

    public class DataEntityGrainInvoker : GrainInvoker
    {
        public DataEntityGrainInvoker(IServiceProvider serviceProvider, MethodInfo methodInfo) : 
            base(serviceProvider, methodInfo)
        {
        }

        protected override Task<object[]> GetParameterList(HttpContext context)
        {
            throw new NotImplementedException();
        }
    }
}
