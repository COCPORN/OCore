using OCore.Services;

namespace OCore.Samples.Minimal.Host
{
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
}