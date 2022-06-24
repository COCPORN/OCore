using OCore.Services;
using System.Threading.Tasks;
using OCore;
using Orleans;

namespace HelloWorld
{
    [Service("HelloWorld")]
    public interface IHelloWorldService : IService
    {
        Task<string> SayHelloTo(string name);
        Task<string> SayHello(HelloRequest request);
    }

    public class HelloWorldService : Service, IHelloWorldService
    {
        public Task<string> SayHelloTo(string name)
        {
            return Task.FromResult($"Hello, {name}!");
        }

        public Task<string> SayHello(HelloRequest request)
        {
            return Task.FromResult($"Hello, {request.Name}!");
        }
    }

    [Serializable]
    [GenerateSerializer]
    public class HelloRequest
    {
        [Id(0)]
        public string Name { get; set; }
    }

    class Program
    {
        static async Task Main(string[] args) =>
           await OCore.Setup.DeveloperExtensions.LetsGo();
    }
}