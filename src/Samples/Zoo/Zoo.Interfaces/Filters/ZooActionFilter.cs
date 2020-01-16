using Microsoft.AspNetCore.Http;
using OCore.Http.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Zoo.Interfaces.Filters
{
    public class ZooActionFilter : Attribute, IActionFilter
    {
        public float Order => 1.0f;

        public void OnActionExecuted(HttpContext context)
        {
            ;
        }

        public void OnActionExecuting(HttpContext context)
        {
            ;
        }
    }
}
