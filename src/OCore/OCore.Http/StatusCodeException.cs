using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace OCore.Http
{
    public class StatusCodeException : Exception
    {


        public StatusCodeException(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
        }

        public StatusCodeException(HttpStatusCode statusCode, 
            string message, 
            Exception innerException)
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; set; }
    }
}
