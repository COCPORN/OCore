using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;

namespace OCore.Http.OpenApi
{
    public class OpenApiHandler
    {

        string Title { get; set; }

        public OpenApiHandler(string title)
        {
            Title = title;
        }

        internal async Task Dispatch(HttpContext context)
        {
            var document = new OpenApiDocument
            {
                Info = new OpenApiInfo
                {
                    Version = "1.0.0",
                    Title = Title,
                },
                Servers = new List<OpenApiServer>
                {
                    new OpenApiServer { Url = "http://petstore.swagger.io/api" }
                },
                Paths = new OpenApiPaths
                {
                    ["/pets"] = new OpenApiPathItem
                    {
                        Operations = new Dictionary<OperationType, OpenApiOperation>
                        {
                            [OperationType.Get] = new OpenApiOperation
                            {
                                Description = "Returns all pets from the system that the user has access to",
                                Responses = new OpenApiResponses
                                {
                                    ["200"] = new OpenApiResponse
                                    {
                                        Description = "OK"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            context.Response.ContentType = "application/json";

            var openApiDocumentation = document.Serialize(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json);
            
            await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(openApiDocumentation), 0, openApiDocumentation.Length);
            await context.Response.Body.FlushAsync();
        }

    }
}
