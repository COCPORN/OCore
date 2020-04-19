using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OCore.Core.Extensions;
using Orleans;
using Orleans.Concurrency;
using Orleans.Streams;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Events
{
    [StatelessWorker]        
    public class EventAggregatorGrain : Grain, IEventAggregator
    {
        readonly ILogger logger;
        EventOptions options;
        

        public EventAggregatorGrain(ILogger<EventAggregatorGrain> logger,
            IOptions<EventOptions> options)
        {
            this.logger = logger;
            this.options = options.Value;
        }
        
        public override Task OnActivateAsync()
        {
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

        void ConfigureFor(Type t, EventTypeOptions options)
        {
            if (options.Destinations != null)
            {
                workerDimensions[t] = options.Destinations;
            }
            else
            {
                if (options.Stable == true)
                {
                    var guids = CreateNControlledGuidsFor(t, options.WorkerInstances);
                    workerDimensions[t] = guids;
                }
                else
                {
                    var guids = CreateNRandomGuids(options.WorkerInstances);
                    workerDimensions[t] = guids;
                }
            }
        }

        private Guid GetDestination<T>()
        {
            if (workerDimensions.TryGetValue(typeof(T), out var list) == true)
            {
                return GetRandomDestination(list);
            }
            else
            {
                throw new InvalidOperationException("No destination found");
            }
        }

        ConcurrentDictionary<Type, (string, EventTypeOptions)> typeOptions = new ConcurrentDictionary<Type, (string, EventTypeOptions)>();

        public async Task Raise<T>(T @event, string streamNameSuffix = null)
        {
            if (typeOptions.TryGetValue(typeof(T), out var eventTypeOptions) == false)
            {
                var eventAttribute = typeof(T).GetCustomAttribute<EventAttribute>(true);
                if (eventAttribute == null)
                {
                    throw new InvalidOperationException("Event class is missing [Event(...)] attribute");
                }
                if (options?.EventTypes != null 
                    && options.EventTypes.TryGetValue(eventAttribute.Name, out var eventTypeConfig))
                {
                    eventTypeOptions = (eventAttribute.Name, eventTypeConfig);
                }
                else
                {
                    eventTypeOptions = (eventAttribute.Name, eventAttribute.Options);
                }

                typeOptions.AddOrUpdate(typeof(T), eventTypeOptions, (key, oldvalue) => eventTypeOptions);
                ConfigureFor(typeof(T), eventTypeOptions.Item2);
            }

            Guid destination = GetDestination<T>();

            var streamName = eventTypeOptions.Item1;

            if (streamNameSuffix != null)
            {
                streamName += $":{streamNameSuffix}";
            }

            var streamProvider = GetStreamProvider(eventTypeOptions.Item2.ProviderName ?? "BaseStreamProvider");

            var stream = streamProvider.GetStream<Event<T>>(destination, streamName);
            if (eventTypeOptions.Item2.FireAndForget == true)
            {
                stream.OnNextAsync(EnvelopeEvent(@event)).FireAndForget(logger);             
            }
            else
            {
                await stream.OnNextAsync(EnvelopeEvent(@event));
            }
        }

        private static Event<T> EnvelopeEvent<T>(T @event)
        {
            return new Event<T>
            {
                Payload = @event,
                CreationTime = DateTimeOffset.UtcNow,
                MessageId = Guid.NewGuid()
            };
        }

    }
}
