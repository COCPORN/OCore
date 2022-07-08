using Orleans;
using System;

namespace OCore.Diagnostics
{
    [Serializable]
    [GenerateSerializer]
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

    /// <summary>
    /// Hello. This is the official documentation: The programming interface for this is confusing at best
    /// </summary>
    [Serializable]
    [GenerateSerializer]
    public class DiagnosticsPayload
    {
        public string CorrelationId { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public int HopCount { get; set; }

        public string PreviousGrainName { get; set; }

        public string PreviousMethodName { get; set; }

        public string GrainName { get; set; }

        public string MethodName { get; set; }

        public string RequestSource { get; set; }

        public override string ToString()
        {
            if (PreviousMethodName != null)
            {
                return $"{PreviousGrainName}.{PreviousMethodName} => {GrainName}.{MethodName} ({HopCount}) {CreatedAt.Ticks} {CorrelationId} {RequestSource}";
            }
            else
            {
                return $"{GrainName}.{MethodName} ({HopCount}) {CreatedAt.Ticks} {CorrelationId} {RequestSource}";
            }

        }
    }
}
