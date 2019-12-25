﻿using OCore.Authorization.Abstractions;
using OCore.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization
{
    [Service("OCore.Token")]
    public interface ITokenService : IService
    {
        [Authorize]
        Task AddToken(Guid token, Guid account);

        [Authorize(permissions: Permissions.None)]
        Task<Guid> GetAccount(Guid token);
    }
}
