using OCore.Authorization.Abstractions;
using OCore.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization
{
    [Service("OCore.System")]
    public interface ISystemService : IService
    {
        /// <summary>
        /// Setup the system for first use using a Guid token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [Authorize(requirements: Requirements.None)]
        Task Initialize(Guid token, Guid accountId);
    }
}
