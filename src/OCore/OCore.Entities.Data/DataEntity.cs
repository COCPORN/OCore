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

        //public async Task<OperationResponse> PerformOperation(OperationRequest request)
        //{
        //    switch (request.Operation)
        //    {
        //        case Operation.Create:
        //            await Create((T)request.Payload);
        //            return new OperationResponse { };
        //        case Operation.Delete:
        //            await Delete();
        //            return new OperationResponse { };
        //        case Operation.Read:
        //            var response = await Read();
        //            return new OperationResponse { Payload = response };
        //    }
        //}

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
                State = (T)data;
                return WriteStateAsync();
            }
            else
            {
                throw new InvalidOperationException("DataEntity not created");
            }
        }

        public Task Upsert(T data)
        {
            State = (T)data;
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
