﻿using OCore.Entities.Data;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Zoo.Interfaces
{
    public class Animal
    {
        public string Name { get; set; }

        public string Species { get; set; }

        public string Noise { get; set; }
    }

    [DataEntity("Animal")]
    public interface IAnimal : IDataEntity<Animal>, IGrainWithStringKey
    {
        Task<string> MakeNoise(int times = 1);
    }
}