using System;
using System.Threading.Tasks;

namespace OCore.Entities.Data
{
    public class DataEntity<T> : Entity<T>, IDataEntity<T> where T : new()
    {
        public Task Create(T data)
        {
            if (Created == false)
            {
                State = data;
                return WriteStateAsync();
            } else
            {
                throw new InvalidOperationException("DataEntity is already created");
            }
        }

        public Task<T> Read()
        {
            if (Created == true)
            {
                return Task.FromResult(State);
            }
            else
            {
                throw new InvalidOperationException("DataEntity not created");
            }
        }

        public Task Update(T data)
        {
            if (Created == true)
            {
                State = data;
                return WriteStateAsync();
            }
            else
            {
                throw new InvalidOperationException("DataEntity not created");
            }
        }

        public Task Upsert(T data)
        {
            State = data;
            return WriteStateAsync();
        }

        Task IDataEntity<T>.Delete()
        {
            if (Created == true)
            {
                return base.Delete();
            } else
            {
                throw new InvalidOperationException("DataEntity not created");
            }
        }
    }
}
