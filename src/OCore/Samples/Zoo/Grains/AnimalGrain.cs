using OCore.Entities.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zoo.Interfaces;

namespace Zoo.Grains
{
    public class AnimalGrain : DataEntity<Animal>, IAnimal
    {
        public Task<string> MakeNoise()
        {
            return Task.FromResult(State.Noise);
        }
    }
}
