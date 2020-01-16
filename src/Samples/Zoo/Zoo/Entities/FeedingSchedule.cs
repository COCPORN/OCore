using OCore.Entities.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zoo.Interfaces;

namespace Zoo.Entities
{
    public class FeedingScheduleEntity : DataEntity<FeedingSchedule>, IFeedingSchedule
    {
    }
}
