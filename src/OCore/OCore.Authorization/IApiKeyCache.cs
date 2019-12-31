using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization
{
    public interface IApiKeyCache : IGrainWithStringKey
    {
        Task<ApiKeyState> GetApiKey();
    }
}
