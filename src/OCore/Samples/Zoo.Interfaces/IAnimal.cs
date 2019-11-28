using OCore.Entities.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Zoo.Interfaces
{
    public class Animal
    {
        public string Name { get; set; }

        public string Kind { get; set; }

        public string Noise { get; set; }
    }


    public interface IAnimal : IDataEntity<Animal>
    {
        Task<string> MakeNoise();
    }
}
