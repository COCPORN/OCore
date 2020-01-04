using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using OCore.Authorization;
using OCore.Entities.Data;

namespace OCore.Http.OpenApi
{
    public class OpenApiHandler
    {

        string Title { get; set; }

        string Version { get; set; }

        bool LoadDocumentationXml { get; set; }

        public OpenApiHandler(string title, string version, bool loadDocumentationXml = true)
        {
            Title = title;
            Version = version;
            LoadDocumentationXml = loadDocumentationXml;
        }

        internal async Task Dispatch(HttpContext context)
        {

            try
            {
                OpenApiDocument document = CreateDocument();

                context.Response.ContentType = "application/json";

                var openApiDocumentation = document.Serialize(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json);

                await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(openApiDocumentation), 0, openApiDocumentation.Length);
                await context.Response.Body.FlushAsync();
            } catch (Exception ex)
            {
                var exc = ex.ToString();
                await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(exc), 0, exc.Length);
            }
        }

        private OpenApiDocument CreateDocument()
        {
            var assemblyLocation = Assembly.GetEntryAssembly().Location;

            var resourceList = ResourceEnumerator.Resources;

            var apiPaths = CreateApiPaths(resourceList);

            return new OpenApiDocument
            {
                Info = new OpenApiInfo
                {
                    Version = Version,
                    Title = Title,
                },
                Servers = new List<OpenApiServer>
                {
                    new OpenApiServer { Url = "http://localhost:9000" }
                },
                Paths = apiPaths
            };
        }

        private OpenApiPaths CreateApiPaths(List<Resource> resourceList)
        {
            var paths = new OpenApiPaths();

            Dictionary<string, List<Resource>> baseResources = new Dictionary<string, List<Resource>>();

            foreach (var resourceEntry in resourceList)
            {
                if (baseResources.TryGetValue(resourceEntry.BaseResource, out var resourceDescriptions))
                {
                    resourceDescriptions.Add(resourceEntry);
                }
                else
                {
                    baseResources.Add(resourceEntry.BaseResource, new List<Resource> { resourceEntry });
                }
            }

            //foreach (var baseResource in baseResources)
            //{
            //    var operations = new Dictionary<OperationType, OpenApiOperation>();

            //    foreach (var resource in baseResource.Value)
            //    {
            //        operations.Add(GetOperationType(resource), GetOperationDescription(resource));
            //    }

            //    OpenApiPathItem item = new OpenApiPathItem
            //    {
            //        Operations = operations
            //    };

            //    paths.Add(baseResource.Key, item);
            //}

            foreach (var resource in resourceList)
            {
                if (resource is ServiceResource)
                {
                    AddServiceResource(paths, resource);
                }
                else if (resource is DataEntityResource dataEntityResource)
                {
                    AddDataEntityResource(paths, dataEntityResource);
                }
            }

            return paths;

        }

        private void AddDataEntityResource(OpenApiPaths paths, DataEntityResource dataEntityResource)
        {
            if (dataEntityResource.BaseResource != dataEntityResource.ResourcePath)
            {
                paths.Add($"/{dataEntityResource.ResourcePath}", new OpenApiPathItem
                {
                    Operations = new Dictionary<OperationType, OpenApiOperation>
                    {
                        {
                            OperationType.Post, new OpenApiOperation
                            {
                                Description = dataEntityResource.BaseResource,
                                Summary = $"{dataEntityResource.MethodInfo.DeclaringType.FullName}.{dataEntityResource.MethodInfo.Name}"
                            }
                        }
                    }
                });
            }
            else
            {
                if (paths.ContainsKey(dataEntityResource.BaseResource) == false)
                {
                    var operations = new Dictionary<OperationType, OpenApiOperation>();

                    if (dataEntityResource.Attribute.DataEntityMethods.HasFlag(DataEntityMethods.Read))
                    {
                        operations.Add(OperationType.Get,
                            new OpenApiOperation
                            {
                                Description = dataEntityResource.BaseResource,
                                Summary = $"{dataEntityResource.MethodInfo.DeclaringType.FullName}.{dataEntityResource.MethodInfo.Name}"
                            });
                    }

                    if (dataEntityResource.Attribute.DataEntityMethods.HasFlag(DataEntityMethods.Create))
                    {
                        operations.Add(OperationType.Post,
                               new OpenApiOperation
                               {
                                   Description = dataEntityResource.BaseResource,
                                   Summary = $"{dataEntityResource.MethodInfo.DeclaringType.FullName}.{dataEntityResource.MethodInfo.Name}"
                               });
                    }

                    if (dataEntityResource.Attribute.DataEntityMethods.HasFlag(DataEntityMethods.Update))
                    {
                        operations.Add(OperationType.Put,
                                new OpenApiOperation
                                {
                                    Description = dataEntityResource.BaseResource,
                                    Summary = $"{dataEntityResource.MethodInfo.DeclaringType.FullName}.{dataEntityResource.MethodInfo.Name}"
                                });
                    }

                    if (dataEntityResource.Attribute.DataEntityMethods.HasFlag(DataEntityMethods.Delete))
                    {
                        operations.Add(OperationType.Delete,
                                new OpenApiOperation
                                {
                                    Description = dataEntityResource.BaseResource,
                                    Summary = $"{dataEntityResource.MethodInfo.DeclaringType.FullName}.{dataEntityResource.MethodInfo.Name}"
                                });
                    }

                    paths.Add($"/{dataEntityResource.BaseResource}", new OpenApiPathItem
                    {
                        Operations = operations
                    });
                }
            }
        }

        private static void AddServiceResource(OpenApiPaths paths, Resource resource)
        {
            paths.Add($"/{resource.ResourcePath}", new OpenApiPathItem
            {
                Operations = new Dictionary<OperationType, OpenApiOperation>
                    {
                        {
                            OperationType.Post, new OpenApiOperation
                            {
                                Description = resource.BaseResource,
                                Summary = $"{resource.MethodInfo.DeclaringType.FullName}.{resource.MethodInfo.Name}",
                                Responses = new OpenApiResponses
                                {
                                    ["200"] = new OpenApiResponse {
                                        Description = GetReturnType(resource)
                                    }
                                }
                            }
                        }
                    },
            });
        }

        private static string GetReturnType(Resource resource)
        {
            var genericArguments = resource.MethodInfo.ReturnType.GetGenericArguments();
            if (genericArguments.Length != 0)
            {
                return genericArguments[0].ToString();
            }
            else
            {
                return "No content";
            }
        }

        private OperationType GetOperationType(Resource resource)
        {
            if (resource is ServiceResource serviceOperation)
            {
                return OperationType.Post;
            }
            else return OperationType.Get;
        }

        private OpenApiOperation GetOperationDescription(Resource resource)
        {
            return new OpenApiOperation
            {

            };
        }
    }
}
