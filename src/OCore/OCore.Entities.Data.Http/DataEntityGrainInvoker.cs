using Microsoft.AspNetCore.Http;
using OCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace OCore.Entities.Data.Http
{
    public enum HttpMethod
    {
        NotSet,
        Get,
        Post,
        Put,
        Delete
    }

    public class DataEntityGrainInvoker : GrainInvoker
    {
        Type entityType;

        public DataEntityGrainInvoker(IServiceProvider serviceProvider, Type grainType, Type interfaceType, MethodInfo methodInfo, Type entityType) :
            base(serviceProvider, grainType, methodInfo)
        {
            this.entityType = entityType;
        }

        public bool IsCrudOperation { get; set; }

        public HttpMethod HttpMethod { get; set; }

        

        protected override async Task<object[]> GetParameterList(HttpContext context)
        {
            if (IsCrudOperation == false)
            {
                return await GetCallParameters(context);
            }
            else
            {
                return await GetCrudParameters(context);
            }
        }

        async Task<object[]> GetCrudParameters(HttpContext context)
        {
            if (HttpMethod == HttpMethod.NotSet)
            {
                throw new InvalidOperationException("HttpMethod is not set for CRUD operation");
            }

            switch (HttpMethod)
            {
                case HttpMethod.Get:
                case HttpMethod.Delete:
                    return new object[] { };
                case HttpMethod.Post:
                case HttpMethod.Put:
                    return await GetCrudBodyEntityParameters(context);
                default:
                    throw new InvalidOperationException("HttpMethod is not set for known CRUD operation");
            }

        }

        private async Task<object[]> GetCrudBodyEntityParameters(HttpContext context)
        {            
            using (var reader = new StreamReader(context.Request.Body))
            {
                var body = await reader.ReadToEndAsync();
                return new object[] { JsonSerializer.Deserialize(body, entityType) };
            }         
        }

        async Task<object[]> GetCallParameters(HttpContext context)
        {
            var parameterList = new List<object>();
            using (var reader = new StreamReader(context.Request.Body))
            {
                var body = await reader.ReadToEndAsync();

                if (string.IsNullOrEmpty(body) == true)
                {
                    AddDefaultParameters(parameterList);
                }
                else if (body[0] == '[')
                {

                    var deserialized = JsonSerializer.Deserialize<object[]>(body);

                    if (deserialized.Length > Parameters.Count)
                    {
                        throw new InvalidOperationException($"Parameter count too high");
                    }

                    int i = 0;
                    foreach (var parameter in deserialized)
                    {
                        parameterList.Add(ProjectValue(parameter, Parameters[i++]));
                    }

                    AddDefaultParameters(parameterList);
                }
                else
                {
                    if (Parameters.Count != 1)
                    {
                        throw new InvalidOperationException($"Parameter count mismatch");
                    }
                    parameterList.Add(JsonSerializer.Deserialize(body, Parameters[0].Type));
                }
            }
            return parameterList.ToArray();
        }

        private void AddDefaultParameters(List<object> parameterList)
        {
            while (parameterList.Count < Parameters.Count)
            {
                parameterList.Add(Type.Missing);
            }
        }
    }
}
