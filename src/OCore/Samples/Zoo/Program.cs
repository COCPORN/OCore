using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using OCore.Setup;

namespace Zoo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var hostBuilder = new HostBuilder();
            await hostBuilder.LetsGo(typeof(Services.Zoo));
            
            Console.ReadLine();
        }

    }
}
