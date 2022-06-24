using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using OCore.Http;
using Orleans;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace OCore.Services.Http
{
    class Parameter
    {
        public Type Type { get; set; }
        public string Name { get; set; }
    }

    public class ServiceGrainInvoker : GrainInvoker
    {

        private static JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true
        };

        public ServiceGrainInvoker(IServiceProvider serviceProvider, Type grainType, MethodInfo methodInfo) : 
            base(serviceProvider, grainType, methodInfo)
        {
        }

        protected override async Task<object[]> GetParameterList(HttpContext context)
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

                    var deserialized = JsonSerializer.Deserialize<object[]>(body, jsonSerializerOptions);

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

                    parameterList.Add(JsonSerializer.Deserialize(body, Parameters[0].Type, jsonSerializerOptions));
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
