﻿using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OCore.Authorization.RateLimiting
{
    class Request
    {
        public DateTimeOffset Time { get; set; }

        public long Size { get; set; }
    }

    public class RateCounter : Grain, IRateCounter
    {
        LinkedList<Request> requests = new LinkedList<Request>();

        void PruneExpiredRequests(ApiKeySettings settings)
        {
            var request = requests.First;
            while (DateTimeOffset.UtcNow - request.Value.Time > settings.Window)
            {
                requests.Remove(request);
                request = request.Next;
            }
        }

        long GetLoad()
        {
            return requests.Sum(x => x.Size);
        }

        public Task<bool> CheckRateLimit(ApiKeySettings settings)
        {
            PruneExpiredRequests(settings);
            return Task.FromResult(GetLoad() > settings.Limit);
        }

        public Task ReportDataUsage(DataUsage dataUsage)
        {
            requests.AddLast(new Request
            {
                Time = DateTimeOffset.UtcNow,
                Size = dataUsage.Bytes
            });
            return Task.CompletedTask;
        }
    }
}
