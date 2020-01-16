using OCore.Entities.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zoo.Interfaces
{
    public class FeedingSchedule
    {

        public bool Feed { get; set; }
    }

    public interface IFeedingSchedule : IDataEntity<FeedingSchedule>
    {
    }
}
