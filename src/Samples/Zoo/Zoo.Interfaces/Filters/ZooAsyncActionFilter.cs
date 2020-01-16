using Microsoft.AspNetCore.Http;
using OCore.Http.Filters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Zoo.Interfaces.Filters
{
    public class ZooAsyncActionFilter : Attribute, IAsyncActionFilter
    {
        public float Order => 0.5f;

        public Task OnActionExecutionAsync(HttpContext context, Func<HttpContext, Task> next)
        {
            ;
            return next(context);            
        }
    }
}
