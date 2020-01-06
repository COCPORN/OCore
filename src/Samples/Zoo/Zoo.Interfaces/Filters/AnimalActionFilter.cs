using Microsoft.AspNetCore.Http;
using OCore.Http.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Zoo.Interfaces.Filters
{
    public class AnimalActionFilter : Attribute, IAsyncActionFilter
    {
        public float Order => 1.0f;

        public async Task OnActionExecutionAsync(HttpContext context, Func<HttpContext, Task> next)
        {
            ;
            await next(context);
        }
    }
}
