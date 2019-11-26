using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Orleans;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace OCore.Service.Http
{
    class Parameter
    {
        public Type Type { get; set; }
        public string Name { get; set; }
    }

    public class GrainInvoker
    {
        public MethodInfo MethodInfo { get; set; }
        MethodInfo getResult;

        private static readonly MethodInfo getResultMethod = typeof(GrainInvoker).GetMethod(nameof(GetResult), BindingFlags.Static | BindingFlags.NonPublic);
        private static object GetResult<T>(Task<T> input) => (object)input.GetAwaiter().GetResult();

        IServiceProvider serviceProvider;
        public Type GrainType => MethodInfo.DeclaringType;

        List<Parameter> parameters = new List<Parameter>();

        public GrainInvoker(IServiceProvider serviceProvider, MethodInfo methodInfo)
        {
            this.MethodInfo = methodInfo;
            this.serviceProvider = serviceProvider;

            BuildParameterMap();
            BuildResultDelegate();
        }

        public async Task Invoke(IGrain grain, HttpContext context)
        {
            object[] parameterList = await GetParameterList(context);
            var grainCall = (Task)MethodInfo.Invoke(grain, parameterList);
            await grainCall;

            if (getResult != null)
            {
                object result = getResult.Invoke(null, new[] { grainCall });
                if (result != null)
                {
                    context.Response.ContentType = "application/json";

                    await Serialize(result, context.Response.BodyWriter);
                }
            }
        }

        private async Task<object[]> GetParameterList(HttpContext context)
        {
            var parameterList = new List<object>();
            using (var reader = new StreamReader(context.Request.Body))
            {
                var body = await reader.ReadToEndAsync();

                if (string.IsNullOrEmpty(body) == true)
                {
                    if (parameters.Count != 0)
                    {
                        throw new InvalidOperationException($"Parameter count mismatch");
                    }
                }
                else if (body[0] == '[')
                {

                    var deserialized = JsonSerializer.Deserialize<object[]>(body);

                    if (deserialized.Length != parameters.Count)
                    {
                        throw new InvalidOperationException($"Parameter count mismatch");
                    }

                    int i = 0;
                    foreach (var parameter in parameters)
                    {
                        parameterList.Add(ProjectValue(deserialized[i++], parameter));
                    }
                }
                else
                {
                    if (parameters.Count != 1)
                    {
                        throw new InvalidOperationException($"Parameter count mismatch");
                    }
                    parameterList.Add(JsonSerializer.Deserialize(body, parameters[0].Type));
                }
            }
            return parameterList.ToArray();
        }

        static Dictionary<Type, Func<object, object>> Converters = new Dictionary<Type, Func<object, object>>()
        {
            { typeof(string), s => s.ToString() },
            { typeof(int), s => int.Parse(s.ToString()) },
            { typeof(DateTime), s => DateTime.Parse(s.ToString()) },
            { typeof(DateTimeOffset), s => DateTimeOffset.Parse(s.ToString()) },
            { typeof(TimeSpan), s => TimeSpan.Parse(s.ToString()) },
            { typeof(double), s => double.Parse(s.ToString()) },
            { typeof(float), s => float.Parse(s.ToString()) },
            { typeof(decimal), s => decimal.Parse(s.ToString()) }
        };

        private object ProjectValue(object deserializedValue, Parameter parameter)
        {
            if (Converters.TryGetValue(parameter.Type, out var converter))
            {
                return converter(deserializedValue);
            }
            else
            {
                // This is no doubt a hack. Let's leave it as is for now
                var serialized = JsonSerializer.Serialize(deserializedValue);
                return JsonSerializer.Deserialize(serialized, parameter.Type);
            }
        }

        public async ValueTask Serialize(object obj, PipeWriter writer)
        {
            await JsonSerializer.SerializeAsync(writer.AsStream(), obj, obj.GetType());
        }

        void BuildResultDelegate()
        {
            if (MethodInfo.ReturnType.IsGenericType
                && MethodInfo.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                getResult = getResultMethod.MakeGenericMethod(MethodInfo.ReturnType.GenericTypeArguments[0]);
            }
        }

        private void BuildParameterMap()
        {
            var parameters = MethodInfo.GetParameters();

            foreach (var parameter in parameters)
            {
                this.parameters.Add(new Parameter
                {
                    Name = parameter.Name,
                    Type = parameter.ParameterType
                });
            }
        }

    }
}
