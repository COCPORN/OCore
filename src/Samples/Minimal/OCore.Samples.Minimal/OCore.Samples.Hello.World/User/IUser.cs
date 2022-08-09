using OCore.Entities.Data;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Samples.Hello.World.User
{
    [Serializable]
    [GenerateSerializer]
    public class UserData
    {

    }

    [DataEntity("User")]
    public interface IUser : IDataEntity<UserData>
    {
    }
}
