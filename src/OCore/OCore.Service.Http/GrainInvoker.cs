using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Orleans;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace OCore.Service.Http
{
    public class GrainInvoker
    {

        MethodInfo methodInfo;
        MethodInfo getResult;

        private static readonly MethodInfo getResultMethod = typeof(GrainInvoker).GetMethod(nameof(GetResult), BindingFlags.Static | BindingFlags.NonPublic);
        private static object GetResult<T>(Task<T> input) => (object)input.GetAwaiter().GetResult();

        IServiceProvider serviceProvider;
        public Type GrainType => methodInfo.DeclaringType;

        public GrainInvoker(IServiceProvider serviceProvider, MethodInfo methodInfo)
        {
            this.methodInfo = methodInfo;
            this.serviceProvider = serviceProvider;

            BuildResultDelegate();
        }

        public async Task Invoke(IGrain grain, HttpContext context)
        {
            var grainCall = (Task)methodInfo.Invoke(grain, new object[] { "Hello" });
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

        public async ValueTask Serialize(object obj, PipeWriter writer)
        {
            await JsonSerializer.SerializeAsync(writer.AsStream(), obj, obj.GetType());
        }

        void BuildResultDelegate()
        {
            if (methodInfo.ReturnType.IsGenericType
                && methodInfo.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                getResult = getResultMethod.MakeGenericMethod(methodInfo.ReturnType.GenericTypeArguments[0]);
            }
        }

    }
}
