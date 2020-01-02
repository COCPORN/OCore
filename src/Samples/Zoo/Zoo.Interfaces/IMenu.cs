using OCore.Entities.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Zoo.Interfaces
{
    public class Menu
    {
        public List<string> FoodItems { get; set; } = new List<string>();
    }

    [DataEntity("Menu", KeyStrategy.Global)]
    public interface IMenu: IDataEntity<Menu>
    {
        Task<int> Totals();
    }
}
