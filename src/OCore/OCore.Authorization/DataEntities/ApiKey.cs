using OCore.Entities.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization.DataEntities
{
    public class ApiKey : DataEntity<ApiKeyState>, IApiKey
    {
        public Task Activate()
        {
            State.IsValid = true;
            return WriteStateAsync();
        }

        public Task Deactivate()
        {
            State.IsValid = false;
            return WriteStateAsync();
        }
    }
}
