using OCore.Service;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Zoo.Services
{
    public class Zoo : Service, IZoo
    {
        public Task<string> Greet(string name)
        {
            return Task.FromResult("Du er en sau");
        }
    }
}
