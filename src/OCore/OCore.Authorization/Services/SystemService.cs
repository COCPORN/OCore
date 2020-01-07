using OCore.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization.Services
{
    public class SystemService : Service, ISystemService
    {
        public async Task Initialize(Guid token, string accountId)
        {
            var systemSetup = GrainFactory.GetGrain<ISystemSetup>(0);

            if (await systemSetup.IsSystemSetup() == true)
            {
                throw new InvalidOperationException("System is already set up");
            }

            var tokenService = GetService<ITokenService>();            
            await tokenService.AddToken(token, accountId);

            // Bypass the role service for this call to make sure authorization
            // in the service doesn't leak
            var accountRoles = GrainFactory.GetGrain<IAccountRoles>(accountId);
            await accountRoles.AddRole("root");

            await systemSetup.SetupSystem();            
        }
    }
}
