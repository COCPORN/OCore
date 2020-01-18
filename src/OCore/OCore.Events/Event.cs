using System;

namespace OCore.Events
{
    public class Event<T>
    {
        public Guid MessageId { get; set; }

        public DateTimeOffset CreationTime { get; set; }

        /// <summary>
        /// This message could not be handled
        /// </summary>
        public bool IsPoisonous { get; set; }

        /// <summary>
        /// This will be populated from the EventCounter when the message is deemed poisonous
        /// </summary>
        public int Retries { get; set; }

        public T Payload { get; set; }
    }
}
