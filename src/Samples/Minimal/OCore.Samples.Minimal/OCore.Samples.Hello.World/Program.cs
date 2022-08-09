using OCore.Entities.Data;
using OCore.Entities.Data.Extensions;
using OCore.Services;
using Orleans;

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
            => Task.FromResult($"Hello, {name}!");

        public async Task<string> ShoutHelloTo(string name)
        {
            var capitalizationService = GetService<INameCapitalizationService>();

            //var lookupCounter = GrainFactory.GetDataEntity<ILookupCounter>(name);
            //await lookupCounter.IncreaseLookups();

            var capitalizedName = await capitalizationService.Capitalize(name);

            var helloWorldService = GetService<IHelloWorldService>();

            return await helloWorldService.SayHelloTo(capitalizedName);
        }
    }

    [Service("Capitalization")]
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

    //[Serializable]
    //[GenerateSerializer]
    //public class LookupData
    //{
    //    [Id(0)]
    //    public int Lookups { get; set; }
    //}


    //[DataEntity("LookupCounter")]
    //public interface ILookupCounter : IDataEntity<LookupData>
    //{
    //    Task IncreaseLookups();
    //}

    //public class LookupCounter : DataEntity<LookupData>, ILookupCounter
    //{
    //    public Task IncreaseLookups()
    //    {
    //        State.Lookups++;
    //        return WriteStateAsync();
    //    }
    //}
}