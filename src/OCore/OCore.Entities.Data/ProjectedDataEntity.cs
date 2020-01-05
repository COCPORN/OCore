using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Entities.Data
{
    public abstract class ProjectedDataEntity<T> : Grain, IDataEntity<T>
    {
        public Task Create(T data)
        {
            throw new NotImplementedException();
        }

        public Task Delete()
        {
            throw new NotImplementedException();
        }

        public abstract Task<T> Read();
        
        public Task Update(T data)
        {
            throw new NotImplementedException();
        }

        public Task Upsert(T data)
        {
            throw new NotImplementedException();
        }
    }
}
