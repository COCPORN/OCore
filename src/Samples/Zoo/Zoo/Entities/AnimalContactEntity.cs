using OCore.Core.Extensions;
using OCore.Entities.Data;
using OCore.Entities.Data.Extensions;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zoo.Interfaces;

namespace Zoo.Grains
{
    public class AnimalContactEntity : Grain, IAnimalContact
    {        

        public Task Create(AnimalContact data)
        {
            throw new NotImplementedException();
        }

        public Task Delete()
        {
            throw new NotImplementedException();
        }

        public async Task<AnimalContact> Read()
        {
            var animalAccountGrain = GrainFactory.GetGrain<IAnimalAccount>(this.GetEntityKeyExtension());
            var account = await animalAccountGrain.Read();
            return new AnimalContact
            {
                FavoriteColor = account.FavoriteColor
            };
        }

        public Task Update(AnimalContact data)
        {
            throw new NotImplementedException();
        }

        public Task Upsert(AnimalContact data)
        {
            throw new NotImplementedException();
        }

    }
}
