using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OCore.Http.Filters
{
    public interface IAsyncActionFilter 
    {        

        Task OnActionExecutionAsync(HttpContext context, Func<HttpContext, Task> next);

        float Order { get; }
    }
}
