using OCore.Services;

namespace OCore.Samples.Minimal.Host
{
    [Service("HelloWorld")]
    public interface IHelloWorldService : IService
    {
        Task<string> SayHelloTo(string name);
        Task<string> SayHello(HelloRequest request);
    }
}