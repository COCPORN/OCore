using OCore.Entities.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zoo.Interfaces;

namespace Zoo.Grains
{
    /// <summary>
    /// This is the account for the animal
    /// </summary>
    public class AnimalAccountEntity : DataEntity<AnimalAccount>, IAnimalAccount
    {

    }
}
