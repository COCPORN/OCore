using Microsoft.AspNetCore.Http;
using Orleans;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace OCore.Http
{
    public class Parameter
    {
        public Type Type { get; set; }
        public string Name { get; set; }
    }

    public abstract class GrainInvoker
    {
        public MethodInfo MethodInfo { get; set; }
        protected MethodInfo GetResult { get; set;  }

        private static readonly MethodInfo getResultMethod = typeof(GrainInvoker).GetMethod(nameof(GetResultDelegate), BindingFlags.Static | BindingFlags.NonPublic);
        private static object GetResultDelegate<T>(Task<T> input) => input.GetAwaiter().GetResult();

        IServiceProvider serviceProvider;
        public Type GrainType => MethodInfo.DeclaringType;

        protected List<Parameter> Parameters = new List<Parameter>();

        public GrainInvoker(IServiceProvider serviceProvider, MethodInfo methodInfo)
        {
            this.MethodInfo = methodInfo;
            this.serviceProvider = serviceProvider;

            BuildParameterMap();
            BuildResultDelegate();
        }


        protected virtual void BuildResultDelegate()
        {
            if (MethodInfo.ReturnType.IsGenericType
                && MethodInfo.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                GetResult = getResultMethod.MakeGenericMethod(MethodInfo.ReturnType.GenericTypeArguments[0]);
            }
        }

        protected virtual void BuildParameterMap()
        {
            var parameters = MethodInfo.GetParameters();

            foreach (var parameter in parameters)
            {
                this.Parameters.Add(new Parameter
                {
                    Name = parameter.Name,
                    Type = parameter.ParameterType
                });
            }
        }


        public async Task Invoke(IGrain grain, HttpContext context)
        {
            object[] parameterList = await GetParameterList(context);
            var grainCall = (Task)MethodInfo.Invoke(grain, parameterList);
            await grainCall;

            if (GetResult != null)
            {
                object result = GetResult.Invoke(null, new[] { grainCall });
                if (result != null)
                {
                    context.Response.ContentType = "application/json";

                    await Serialize(result, context.Response.BodyWriter);
                }
            }
        }

        public async ValueTask Serialize(object obj, PipeWriter writer)
        {
            await JsonSerializer.SerializeAsync(writer.AsStream(), obj, obj.GetType());
        }

        protected abstract Task<object[]> GetParameterList(HttpContext context);

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

        protected object ProjectValue(object deserializedValue, Parameter parameter)
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

    }
}
