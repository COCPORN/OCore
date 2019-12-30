using OCore.Entities.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zoo.Interfaces;

namespace Zoo.Grains
{
    public class MenuEntity : DataEntity<Menu>, IMenu
    {
        public Task<int> Totals()
        {
            return Task.FromResult(State.FoodItems.Select(x => x.Length).Sum());
        }
    }
}
