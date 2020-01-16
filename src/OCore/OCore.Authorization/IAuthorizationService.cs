using OCore.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization
{
    [Service("OCore.Authorization")]
    public interface IAuthorizationService : IService
    {
        
        [Authorize]
        Task<string> Hello();


    }
}
