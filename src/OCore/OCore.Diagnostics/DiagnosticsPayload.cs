using Orleans;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace OCore.Diagnostics
{
    public enum RequestSource
    {
        /// <summary>
        /// The origin of the request is not set
        /// </summary>
        Unset,

        /// <summary>
        /// The request is created in the filter because it is missing
        /// </summary>
        Filter,

        /// <summary>
        /// The request is created by an OCore.Service entry point (like HTTP)
        /// </summary>
        Service,

        /// <summary>
        /// The request is created by an OCore.DataEntity entry point (like HTTP)
        /// </summary>
        DataEntity,

        /// <summary>
        /// The request is created by a Timer callback
        /// </summary>
        Timer,

        /// <summary>
        /// Some other entity created the request
        /// </summary>
        Other
    }

    public class DiagnosticsPayload
    {
        public string CorrelationId { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        //public IGrainCallContext GrainCallContext { get; set; }

        public int HopCount { get; set; }

        public RequestSource RequestSource { get; set; }

        public static DiagnosticsPayload Register(Action<DiagnosticsPayload> configure)
        {
            var payload = new DiagnosticsPayload
            {
                CorrelationId = Guid.NewGuid().ToString(),
                CreatedAt = DateTimeOffset.UtcNow,
            };
            configure(payload);            
            RequestContext.Set("D", payload);
            return payload;
        }

        public static DiagnosticsPayload GetOrDefault()
        {
            return RequestContext.Get("D") as DiagnosticsPayload;
        }

        public override string ToString() => $"{CreatedAt}/{CorrelationId}: {RequestSource}:{HopCount}";
    }
}
