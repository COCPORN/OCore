using OCore.Authorization;
using OCore.Entities.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zoo.Interfaces;

namespace Zoo.Grains
{
    [Authorize]
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

        public override Task Update(Animal data)
        {
            CheckAnimalData(data);
            return base.Update(data);
        }

        private static void CheckAnimalData(Animal data)
        {
            if (string.IsNullOrEmpty(data.Noise))
            {
                throw new InvalidOperationException("All animals must make noise");
            }
        }

        public override Task Upsert(Animal data)
        {
            CheckAnimalData(data);
            return base.Upsert(data);
        }
    }
}
