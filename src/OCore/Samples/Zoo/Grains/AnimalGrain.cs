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
        public Task<string> MakeNoise(int times)
        {
            List<string> noises = new List<string>();
            for (int i = 0; i < times; i++)
            {
                noises.Add(State.Noise);
            }
            return Task.FromResult(string.Join(", ", noises));
        }
    }
}
