namespace OCore.Samples.Minimal.Host
{
    class Program
    {
        static async Task Main(string[] args) =>
           await OCore.Setup.DeveloperExtensions.LetsGo();
    }
}