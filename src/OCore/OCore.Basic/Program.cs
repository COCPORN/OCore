using OCore.Entities.Data;
using OCore.Services;
using Orleans;

namespace OCore.Sample
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

    [Serializable]
    [GenerateSerializer]
    public class ShortenedUrl
    {
        [Id(0)]
        public string RedirectTo { get; set; }

        [Id(1)]
        public int TimesVisited { get; set; }
    }

    [DataEntity("ShortenedUrl", keyStrategy: KeyStrategy.Identity, dataEntityMethods: DataEntityMethods.All)]
    public interface IShortenedUrl : IDataEntity<ShortenedUrl>
    {
        Task<string> Visit();
    }

    public class ShortenedUrlEntity : DataEntity<ShortenedUrl>, IShortenedUrl
    {
        public async Task<string> Visit()
        {
            State.TimesVisited++;
            await WriteStateAsync();
            return State.RedirectTo;
        }
    }

    class Program
    {
        static async Task Main(string[] args) =>
           await OCore.Setup.DeveloperExtensions.LetsGo();
    }
}