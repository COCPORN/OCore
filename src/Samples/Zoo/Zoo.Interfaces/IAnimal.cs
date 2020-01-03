using OCore.Authorization;
using OCore.Entities.Data;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Zoo.Interfaces
{
    /// <summary>
    /// The description of the animal
    /// </summary>
    public class Animal
    {
        /// <summary>
        /// The given name for the animal
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The species of animal
        /// </summary>
        public string Species { get; set; }

        /// <summary>
        /// The noise this animal makes
        /// </summary>
        public string Noise { get; set; }
    }
    
    /// <summary>
    /// The data entity Animel
    /// </summary>
    [DataEntity("Animal")]
    public interface IAnimal : IDataEntity<Animal>
    {
        /// <summary>
        /// Have this animal make its signature noise
        /// </summary>
        /// <param name="times">Number of times you want this noise to be made</param>
        /// <returns></returns>
        Task<string> MakeNoise(int times = 1);
    }
}
