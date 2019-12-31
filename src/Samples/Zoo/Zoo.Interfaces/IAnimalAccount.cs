using OCore.Authorization;
using OCore.Entities.Data;
using OCore.Entities.Data.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Zoo.Interfaces
{
    public class AnimalAccount
    {
        public string FavoriteColor { get; set; }        

        public string SocialPreference { get; set; }
    }

    [Authorize]
    [DataEntity("AnimalAccount", KeyStrategy.Account)]
    public interface IAnimalAccount : IDataEntity<AnimalAccount>
    {
    }
}
