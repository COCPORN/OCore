using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OCore.Services.Http.Options;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Diagnostics.Middleware
{
    public class CorrelationIdProviderMiddleware
    {
        private readonly RequestDelegate next;

        public CorrelationIdProviderMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context, IOptions<HttpOptions> options /* other dependencies */)
        {

            //if (RequestContext.Get("D:CorrelationId") is string c


            //// I have a feeling this can be improved by not using FirstOrDefault
            //var correlationId = context.Request.Headers.FirstOrDefault(x => x.Key == correlationIdKeyName).Value.ToString();

            //if (string.IsNullOrEmpty(correlationId) == true)
            //{
            //    correlationId = Guid.NewGuid().ToString();
            //}

            //RequestContext.Set("D:CorrelationId", correlationId);

            string? correlationId = null;


            var correlationIdKeyName = options.Value.CorrelationIdHeader;

            if (correlationIdKeyName == null)
            {
                correlationIdKeyName = "correlationid";
            }

            if (context.Request.Headers.TryGetValue(correlationIdKeyName, out var correlationIdHeader))
            {
                correlationId = correlationIdHeader.FirstOrDefault();
            }

            if (correlationId == null)
            {
                correlationId = Guid.NewGuid().ToString();
            }

            RequestContext.Set("D:CorrelationId", correlationId);

            await next(context);
        }
    }
}
