using OCore.Authorization;
using OCore.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Zoo.Interfaces
{
    [Service("Nebb")]
    public interface INebb : IService
    {
        [Authorize]
        Task Zaaap();
    }
}
