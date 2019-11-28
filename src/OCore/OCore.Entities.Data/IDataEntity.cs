using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Entities.Data
{
    public interface IDataEntity { }

    public interface IDataEntity<T> : IDataEntity, IGrainWithStringKey
    {
        /// <summary>
        /// Create new data entity. This call will fail if the entity already exists.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task Create(T data);

        /// <summary>
        /// Read data from entity. This call will fail if the entity does not exist.
        /// </summary>
        /// <returns></returns>
        Task<T> Read();

        /// <summary>
        /// Update the data in the entity. This call will fail is the entity does not exist.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task Update(T data);

        /// <summary>
        /// Update the data if the entity exists or create the entity if it does not exist.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task Upsert(T data);

        /// <summary>
        /// Delete the entity. This call will fail if the entity does not exist.
        /// </summary>
        /// <returns></returns>
        Task Delete();
    }
}
