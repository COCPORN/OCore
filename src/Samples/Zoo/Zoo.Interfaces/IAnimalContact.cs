using OCore.Authorization;
using OCore.Entities.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Zoo.Interfaces
{
    public class AnimalContact
    {
        public string FavoriteColor { get; set; }
    }

    [Authorize]
    [DataEntity("AnimalContact", KeyStrategy.AccountPrefix)]
    public interface IAnimalContact : IDataEntity<AnimalContact>
    {
    }
}
