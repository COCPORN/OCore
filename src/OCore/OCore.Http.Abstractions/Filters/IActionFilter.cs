using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OCore.Http.Filters
{
    public interface IActionFilter
    {
        void OnActionExecuting(HttpContext context);
        void OnActionExecuted(HttpContext context);
        float Order { get; }
    }
}
