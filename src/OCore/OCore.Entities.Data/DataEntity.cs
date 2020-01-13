using Orleans;
using System;
using System.Threading.Tasks;

namespace OCore.Entities.Data
{

    public class DataEntity<T> : Entity<T>, IDataEntity<T> where T : new()
    {
     
        public virtual Task Create(T data)
        {
            if (Created == false)
            {
                State = data;
                return WriteStateAsync();
            }
            else
            {
                throw new DataCreationException($"DataEntity already created: {this.GetPrimaryKeyString()}/{typeof(T)}");
            }
        }

        public virtual Task<T> Read()
        {
            if (Created == true)
            {
                return Task.FromResult(State);
            }
            else
            {
                throw new DataCreationException("DataEntity not created");
            }
        }

        public virtual Task Update(T data)
        {
            if (Created == true)
            {
                State = (T)data;
                return WriteStateAsync();
            }
            else
            {
                throw new DataCreationException("DataEntity not created");
            }
        }

        public virtual Task Upsert(T data)
        {
            State = (T)data;
            return WriteStateAsync();
        }

        Task IDataEntity<T>.Delete()
        {
            if (Created == true)
            {
                return Delete();
            }
            else
            {
                throw new DataCreationException("DataEntity not created");
            }
        }
    }
}
