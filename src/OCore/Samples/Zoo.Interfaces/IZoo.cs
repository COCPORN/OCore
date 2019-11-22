using OCore.Service;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Zoo.Services
{
    [Service("Zoo")]
    public interface IZoo : IService
    {
        Task<string> Greet(string name);
    }
}
