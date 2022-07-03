using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using OCore.Authorization;
using OCore.Entities.Data;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OCore.Http.OpenApi
{
    public class OpenApiHandler
    {

        string Title { get; set; }
        string Version { get; set; }
        bool LoadDocumentationXml { get; set; }
        bool StripInternal { get; set; }
        string servicePrefix { get; set; }
        string dataEntityPrefix { get; set; }

        public OpenApiHandler(string title,
            string version,
            bool stripInternal,
            bool loadDocumentationXml = true,
            string servicePrefix = "/services",
            string dataEntityPrefix = "/data")
        {
            Title = title;
            Version = version;
            StripInternal = stripInternal;
            LoadDocumentationXml = loadDocumentationXml;
            this.servicePrefix = servicePrefix;
            this.dataEntityPrefix = dataEntityPrefix;
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
            }
            catch (Exception ex)
            {
                var exc = ex.ToString();
                await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(exc), 0, exc.Length);
            }
        }

        private OpenApiDocument CreateDocument()
        {
            var assemblyLocation = Assembly.GetEntryAssembly().Location;

            var resourceList = ResourceEnumerator.Resources;
            var resolver = new JsonSerializerDataContractResolver(new JsonSerializerOptions());
            var schemaGenerator = new SchemaGenerator(new SchemaGeneratorOptions(), resolver);
            var schemaRepository = new SchemaRepository();

            var apiPaths = CreateApiPaths(resourceList, schemaGenerator, schemaRepository);

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
                Paths = apiPaths,
                Components = new OpenApiComponents()
                {
                    Schemas = schemaRepository.Schemas
                }
            };
        }

        private OpenApiPaths CreateApiPaths(List<Resource> resourceList, SchemaGenerator schemaGenerator, SchemaRepository schemaRepository)
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

            foreach (var resource in resourceList)
            {
                if (StripInternal == true
                    && resource.BaseResource.StartsWith("OCore")) continue;
                if (resource is ServiceResource)
                {
                    AddServiceResource(paths, resource, servicePrefix, schemaGenerator, schemaRepository);
                }
                else if (resource is DataEntityResource dataEntityResource)
                {
                    AddDataEntityResource(paths, dataEntityResource, dataEntityPrefix, schemaGenerator, schemaRepository);
                }
            }

            return paths;

        }

        private void AddDataEntityResource(OpenApiPaths paths, DataEntityResource dataEntityResource, string dataEntityPrefix, SchemaGenerator schemaGenerator, SchemaRepository schemaRepository)
        {
            if (dataEntityResource.BaseResource != dataEntityResource.ResourcePath)
            {
                paths.Add($"{dataEntityPrefix}/{dataEntityResource.ResourcePath}", new OpenApiPathItem
                {
                    Operations = new Dictionary<OperationType, OpenApiOperation>
                    {
                        {
                            OperationType.Post, new OpenApiOperation
                            {
                                RequestBody = GetRequestType(dataEntityResource.MethodInfo, schemaGenerator, schemaRepository),
                                Tags = new List<OpenApiTag> { new OpenApiTag { Name = dataEntityResource.BaseResource, Description = "DataEntity" } },
                                Description = dataEntityResource.BaseResource,
                                Summary = $"{dataEntityResource.MethodInfo.DeclaringType.Name}.{dataEntityResource.MethodInfo.Name}",
                                Responses = new OpenApiResponses
                                {
                                    ["200"] = GetResponseType(dataEntityResource.MethodInfo, schemaGenerator, schemaRepository)
                                }
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
                    var dataEntityInterface = dataEntityResource.MethodInfo.DeclaringType.GenericTypeArguments[0];

                    if (dataEntityResource.Attribute.DataEntityMethods.HasFlag(DataEntityMethods.Read))
                    {
                        var responses = new OpenApiResponses
                        {
                            ["200"] = GetDataEntityStateResponseType(dataEntityResource.MethodInfo, schemaGenerator, schemaRepository)
                        };
                        operations.Add(OperationType.Get,
                            new OpenApiOperation
                            {
                                Responses = responses,
                                Tags = new List<OpenApiTag> { new OpenApiTag { Name = dataEntityResource.BaseResource, Description = "DataEntity" } },
                                Description = dataEntityResource.BaseResource,
                                Summary = $"{dataEntityInterface.Name}.{DataEntityMethods.Read}"
                            });
                    }

                    if (dataEntityResource.Attribute.DataEntityMethods.HasFlag(DataEntityMethods.Create))
                    {
                        operations.Add(OperationType.Post,
                               new OpenApiOperation
                               {
                                   Tags = new List<OpenApiTag> { new OpenApiTag { Name = dataEntityResource.BaseResource, Description = "DataEntity" } },
                                   Description = dataEntityResource.BaseResource,
                                   Summary = $"{dataEntityInterface.Name}.{DataEntityMethods.Create}"
                               });
                    }

                    if (dataEntityResource.Attribute.DataEntityMethods.HasFlag(DataEntityMethods.Update))
                    {
                        operations.Add(OperationType.Put,
                                new OpenApiOperation
                                {
                                    Tags = new List<OpenApiTag> { new OpenApiTag { Name = dataEntityResource.BaseResource, Description = "DataEntity" } },
                                    Description = dataEntityResource.BaseResource,
                                    Summary = $"{dataEntityInterface.Name}.{DataEntityMethods.Update}"
                                });
                    }

                    if (dataEntityResource.Attribute.DataEntityMethods.HasFlag(DataEntityMethods.Delete))
                    {
                        operations.Add(OperationType.Delete,
                                new OpenApiOperation
                                {
                                    Tags = new List<OpenApiTag> { new OpenApiTag { Name = dataEntityResource.BaseResource, Description = "DataEntity" } },
                                    Description = dataEntityResource.BaseResource,
                                    Summary = $"{dataEntityInterface.Name}.{DataEntityMethods.Delete}"
                                });
                    }

                    paths.Add($"/{dataEntityResource.BaseResource}", new OpenApiPathItem
                    {
                        Operations = operations
                    });
                }
            }
        }

        private static void AddServiceResource(OpenApiPaths paths, Resource resource, string servicePrefix, SchemaGenerator schemaGenerator, SchemaRepository schemaRepository)
        {
            paths.Add($"{servicePrefix}/{resource.ResourcePath}", new OpenApiPathItem
            {
                Operations = new Dictionary<OperationType, OpenApiOperation>
                    {
                        {
                            OperationType.Post, new OpenApiOperation
                            {
                                Tags = new List<OpenApiTag> { new OpenApiTag { Name = resource.BaseResource, Description = "Service" } },
                                Description = resource.BaseResource,
                                RequestBody = GetRequestType(resource.MethodInfo, schemaGenerator, schemaRepository),
                                Summary = $"{resource.MethodInfo.DeclaringType.Name}.{resource.MethodInfo.Name}",
                                Responses = new OpenApiResponses
                                {
                                    ["200"] = GetResponseType(resource.MethodInfo, schemaGenerator, schemaRepository)
                                }
                            }
                        }
                    },
            });
        }

        private static OpenApiRequestBody GetRequestType(MethodInfo methodInfo, SchemaGenerator schemaGenerator, SchemaRepository schemaRepository)
        {
            var firstParameter = methodInfo.GetParameters().Where(type =>
                !type.ParameterType.IsPrimitive &&
                !type.ParameterType.IsSubclassOf(typeof(System.ValueType)) &&
                !type.ParameterType.Equals(typeof(string))).FirstOrDefault();
            if (firstParameter != null)
            {
                schemaGenerator.GenerateSchema(firstParameter.ParameterType, schemaRepository, methodInfo, firstParameter);
                OpenApiSchema schema;
                schemaRepository.TryLookupByType(firstParameter.ParameterType, out schema);
                var body = new OpenApiRequestBody();
                var json = new OpenApiMediaType { Schema = schema };
                body.Content["application/json"] = json;
                return body;
            }
            else
            {
                var schema = new OpenApiSchema
                {
                    Type = "array",
                    Properties = new Dictionary<string, OpenApiSchema>(),
                    Required = new SortedSet<string>(),
                    AdditionalPropertiesAllowed = true
                };

                var body = new OpenApiRequestBody();
                body.Content["application/json"] = new OpenApiMediaType { Schema = schema };
                return body;
            }
        }

        private static Type GetDataEntityInterfaceType(MethodInfo methodInfo)
        {
            Type entityInterface = methodInfo.DeclaringType;
            if (methodInfo.ReflectedType.GenericTypeArguments.Any())
            {
                entityInterface = methodInfo.ReflectedType.GenericTypeArguments.FirstOrDefault();
            }

            var dataInterface = entityInterface.GetInterfaces()?.FirstOrDefault(type => type.IsGenericType &&
                     type.GetGenericTypeDefinition() == typeof(IDataEntity<>) &&
                    typeof(IDataEntity).IsAssignableFrom(type));

            return dataInterface;
        }

        private static OpenApiResponse GetDataEntityStateResponseType(MethodInfo methodInfo, SchemaGenerator schemaGenerator, SchemaRepository schemaRepository)
        {
            var dataEntityInterface = GetDataEntityInterfaceType(methodInfo);
            return GetOrGenerateResponseType(dataEntityInterface.GenericTypeArguments.FirstOrDefault(), methodInfo, schemaGenerator, schemaRepository);
        }

        private static OpenApiResponse GetResponseType(MethodInfo methodInfo, SchemaGenerator schemaGenerator, SchemaRepository schemaRepository)
        {
            var returnType = methodInfo.ReturnType.GetGenericArguments().FirstOrDefault();
            if (returnType == null)
            {
                return new OpenApiResponse() { Description = "No content" };
            }

            return GetOrGenerateResponseType(returnType, methodInfo, schemaGenerator, schemaRepository);
        }

        private static OpenApiResponse GetOrGenerateResponseType(Type type, MethodInfo methodInfo, SchemaGenerator schemaGenerator, SchemaRepository schemaRepository)
        {
            schemaGenerator.GenerateSchema(type, schemaRepository, methodInfo);
            OpenApiSchema schema;
            schemaRepository.TryLookupByType(type, out schema);

            var response = new OpenApiResponse();
            response.Content["application/json"] = new OpenApiMediaType
            {
                Schema = schema
            };
            response.Description = type.Name;
            return response;
        }
    }
}
