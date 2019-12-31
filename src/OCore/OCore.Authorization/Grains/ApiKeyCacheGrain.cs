using Orleans;
using Orleans.Concurrency;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization.Grains
{
    [StatelessWorker]
    public class ApiKeyCacheGrain : Grain, IApiKeyCache
    {
        ApiKeyState key;

        IDisposable timer;

        public async override Task OnActivateAsync()
        {
            await RefreshKey(null);
            timer = RegisterTimer(RefreshKey, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
            await base.OnActivateAsync();
        }

        private async Task RefreshKey(object arg)
        {            
            var apiKey = this.GetPrimaryKey();
            var apiKeyGrain = GrainFactory.GetGrain<IApiKey>(apiKey.ToString());
            key = await apiKeyGrain.Read();
        }

        public async Task<ApiKeyState> GetApiKey()
        {
            if (key.IsValid == true)
            {
                return key;
            }
            else
            {
                DeactivateOnIdle();
                await Task.Delay(1000);
                return null;
            }
        }

    }
}
