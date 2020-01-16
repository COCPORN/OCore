using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OCore.Core.Extensions;
using Orleans;
using Orleans.Concurrency;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Events
{
    [StatelessWorker]
    public class EventAggregatorGrain : Grain, IEventAggregator
    {
        readonly ILogger logger;
        EventOptions options;
        HashSet<Type> fireAndForget = new HashSet<Type>();

        public EventAggregatorGrain(ILogger<EventAggregatorGrain> logger,
            IOptions<EventOptions> options)
        {
            this.logger = logger;
            this.options = options.Value;

            if (this.options.StripFromEventStreamName == null)
            {
                this.options.StripFromEventStreamName = new string[] { };
            }

            // TODO: Rewrite this to scan assemblies and map them to configuration
            // rather than having this the other way around
            //foreach (var kvp in this.options.EventTypes)
            //{
            //    var type = Type.GetType($"{this.options.StripFromEventStreamName}{kvp.Key}, OCore.Application.Events", false);
            //    if (type != null)
            //    {

            //        ConfigureFor(type, kvp.Value.WorkerInstances, kvp.Value.Stable, kvp.Value.Destinations);

            //        if (kvp.Value.FireAndForget == true)
            //        {
            //            fireAndForget.Add(type);
            //        }
            //    }
            //}
        }

        IStreamProvider streamProvider;
        public override Task OnActivateAsync()
        {
            streamProvider = GetStreamProvider("BaseStreamProvider");
            return base.OnActivateAsync();
        }

        Dictionary<Type, List<Guid>> workerDimensions = new Dictionary<Type, List<Guid>>();

        List<Guid> CreateNRandomGuids(int n)
        {
            var guids = new List<Guid>(n);
            for (int i = 0; i < n; i++)
            {
                guids.Add(Guid.NewGuid());
            }
            return guids;
        }

        List<Guid> CreateNControlledGuidsFor(Type t, int n)
        {
            var guids = new List<Guid>(n);
            for (int i = 0; i < n; i++)
            {
                guids.Add($"{t.FullName}:{i}".ToGuid());
            }
            return guids;
        }

        Random rnd = new Random();

        Guid GetRandomDestination(List<Guid> guids)
        {
            var index = rnd.Next(0, guids.Count);
            return guids[index];
        }

        static bool TryChomp(string str, out string result)
        {
            var index = str.LastIndexOf('.');
            if (index == -1)
            {
                result = str;
                return false;
            }
            result = str.Substring(0, index);
            return true;
        }

        Task ConfigureFor(Type t, int capacity, bool stable, List<Guid> destinations)
        {
            if (destinations != null)
            {
                workerDimensions[t] = destinations;
            }
            else
            {
                if (stable == true)
                {
                    var guids = CreateNControlledGuidsFor(t, capacity);
                    workerDimensions[t] = guids;
                }
                else
                {
                    var guids = CreateNRandomGuids(capacity);
                    workerDimensions[t] = guids;
                }
            }
            return Task.CompletedTask;
        }

        List<Guid> fallbackDestinations = null;

        private Guid GetDestination<T>()
        {
            Guid destination;
            if (workerDimensions.TryGetValue(typeof(T), out var list))
            {
                destination = GetRandomDestination(list);
            }
            else if (workerDimensions.TryGetValue(typeof(DefaultWorkerConfiguration), out list))
            {
                destination = GetRandomDestination(list);
            }
            else
            {
                if (fallbackDestinations == null)
                {
                    fallbackDestinations = CreateNControlledGuidsFor(GetType(), 4);
                }
                destination = GetRandomDestination(fallbackDestinations);
            }

            return destination;
        }

        public Task Raise<T>(T @event, string streamNameSuffix = null)
        {
            Guid destination = GetDestination<T>();
            var typename = typeof(T).FullName;
            if (options.StripFromEventStreamName != null)
            {
                foreach (var stripPattern in options.StripFromEventStreamName)
                {
                    typename = typename.Replace(stripPattern, "");
                }
            }

            var streamName = $"{options.StreamNamePrefix}{typename}";
            if (streamNameSuffix != null)
            {
                streamName += $":{streamNameSuffix}";
            }

            var stream = streamProvider.GetStream<EventEnvelope<T>>(destination, streamName);
            if (fireAndForget.Contains(typeof(T)))
            {
                stream.OnNextAsync(EnvelopeEvent<T>(@event)).FireAndForget(logger);
                return Task.CompletedTask;
            }
            else
            {
                return stream.OnNextAsync(EnvelopeEvent<T>(@event));
            }
        }

        private static EventEnvelope<T> EnvelopeEvent<T>(T @event)
        {
            return new EventEnvelope<T>
            {
                Payload = @event,
                CreationTime = DateTimeOffset.UtcNow,
                MessageId = Guid.NewGuid()
            };
        }
    }
}
