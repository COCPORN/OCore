using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Events
{
    public class EventHandler<T> : Grain,
        IAsyncObserver<Event<T>>
    {
        EventOptions options;
        ILogger logger;

        public EventHandler(IOptions<EventOptions> options,
            ILogger<EventHandler<T>> logger)
        {
            this.options = options.Value;
            this.logger = logger;
        }

        EventAttribute eventAttribute = null;
        EventAttribute EventAttribute
        {
            get
            {
                if (eventAttribute == null)
                {
                    eventAttribute = typeof(T).GetCustomAttribute<EventAttribute>();
                    if (eventAttribute == null)
                    {
                        throw new InvalidOperationException("EventHandler must have an [EventHandler]-attribute");
                    }
                }
                return eventAttribute;
            }
        }

        EventTypeOptions eventTypeOptions = null;
        async Task<EventTypeOptions> GetEventTypeOptions()
        {
            if (eventTypeOptions == null)
            {
                if (EventAttribute.OptionsCreator != null)
                {
                    return await EventAttribute.OptionsCreator(EventAttribute.Options);
                }
                else
                {
                    return eventTypeOptions = EventAttribute.Options;

                }
            }
            return eventTypeOptions;
        }

        EventHandlerAttribute EventHandlerAttribute
        {
            get
            {
                return GetType().GetCustomAttribute<EventHandlerAttribute>();
            }
        }

        string GetProviderName()
        {
            var providerName = EventAttribute?.Options?.ProviderName ?? options?.DefaultProviderName;
            if (providerName == null)
            {
                throw new InvalidOperationException("No provider names configured for event handling");
            }
            return providerName;
        }

        public async override Task OnActivateAsync()
        {
            var streamProvider = GetStreamProvider(GetProviderName());
            var stream = streamProvider.GetStream<Event<T>>(this.GetPrimaryKey(), FormatStreamNamespace(EventHandlerAttribute));
            await stream.SubscribeAsync(this);
        }

        private string FormatStreamNamespace(EventHandlerAttribute eventHandlerAttribute)
        {
            if (eventHandlerAttribute.Suffix == null)
            {
                return eventHandlerAttribute.EventName;
            }
            else
            {
                return $"{eventHandlerAttribute.EventName}:{eventHandlerAttribute.Suffix}";
            }
        }

        protected virtual Task HandleEvent(T @event, StreamSequenceToken token = null)
        {
            return Task.CompletedTask;
        }

        protected virtual Task HandleEvent(Event<T> @event, StreamSequenceToken token = null)
        {
            return Task.CompletedTask;
        }

        public async Task OnNextAsync(Event<T> item, StreamSequenceToken token = null)
        {            
            try
            {
                await CallHandlers(item);
            }
            catch
            {
                var eventTypeOptions = await GetEventTypeOptions();


                bool poisonLimitReached = false;

                if (eventTypeOptions.TrackAndKillPoisonEvents == true)
                {
                    var failureTracker = GrainFactory.GetGrain<IPoisonEventCounter>(item.MessageId);
                    var failures = await failureTracker.Handle();
                    if (failures == eventTypeOptions.PoisonLimit)
                    {
                        var eventAggregator = GrainFactory.GetGrain<IEventAggregator>(0);
                        await eventAggregator.Raise(new PoisonEvent<T>(item), "poison");
                    }
                }

                bool @throw = poisonLimitReached == true 
                              || EventHandlerAttribute.ContainExceptions == false;

                if (@throw == true)
                {
                    throw;
                }
            }
        }

        private async Task CallHandlers(Event<T> item)
        {
            await HandleEvent(item);
            await HandleEvent(item.Payload);
        }

        public Task OnCompletedAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            return Task.CompletedTask;
        }
    }
}
