using OCore.Services;
await OCore.Setup.DeveloperExtensions.LetsGo();

namespace HelloWorld
{
    [Service("HelloWorld")]
    public interface IHelloWorldService : IService
    {
        Task<string> SayHelloTo(string name);

        Task<string> ShoutHelloTo(string name);
    }

    public class HelloWorldService : Service, IHelloWorldService
    {
        public Task<string> SayHelloTo(string name)
        {
            return Task.FromResult($"Hello, {name}! It is a beautiful world! And you are my favorite part of it!");
        }

        public async Task<string> ShoutHelloTo(string name)
        {
            var capitalizationService = GetService<INameCapitalizationService>();

            var capitalizedName = await capitalizationService.Capitalize(name);

            return $"Hello, {capitalizedName}! It is a beautiful world! And you are my favorite part of it!";
        }
    }

    [Service("CapitalizationService")]
    public interface INameCapitalizationService : IService
    {
        Task<string> Capitalize(string name);
    }

    public class NameCapitalizationService : Service, INameCapitalizationService
    {
        public Task<string> Capitalize(string name)
        {
            return Task.FromResult(name.ToUpper());
        }
    }
}