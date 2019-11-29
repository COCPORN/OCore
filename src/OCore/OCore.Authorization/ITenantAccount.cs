using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Authorization
{
    interface ITenantAccount : IGrainWithGuidCompoundKey
    {
        Task Create(Guid accountId);

        Task Delete();
    }
}
