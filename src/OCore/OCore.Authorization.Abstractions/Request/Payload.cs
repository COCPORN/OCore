using Orleans;
using Orleans.Runtime;
using System;
using System.Collections.Generic;

namespace OCore.Authorization.Abstractions.Request
{
    [Serializable]
    [GenerateSerializer]
    public class Payload
    {
        [Id(0)]
        public bool IsInitialStateSatisfied { get; set; }
        [Id(1)]
        public Permissions InitialPermissions { get; set; }
        [Id(2)]
        public Requirements InitialRequirements { get; set; }
        [Id(3)]
        public bool AllowElevatedRequests { get; set; }
        [Id(4)]
        public bool ElevateRequest { get; set; }
        [Id(5)]
        public bool IsCompleted { get; set; }
        [Id(6)]
        public string Token { get; set; }
        [Id(7)]
        public string ApiKey { get; set; }
        [Id(8)]
        public List<string> ApiKeyApplications { get; set; }
        [Id(9)]
        public string OriginalAccountId { get; set; }
        [Id(10)]
        public string ProjectedAccountId { get; set; }
        public string AccountId => ProjectedAccountId != null ? ProjectedAccountId : OriginalAccountId;
        [Id(11)]
        public bool AccountIdHasBeenProjected { get; set; }
        [Id(12)]
        public string TenantId { get; set; }
        [Id(13)]
        public bool IsRequestElevated { get; set; }
        [Id(14)]
        public List<string> Roles { get; set; }

        public static Payload Get()
        {
            var payload = RequestContext.Get("I") as Payload;
            if (payload == null)
            {
                throw new UnauthorizedAccessException("No identity payload was found in the request context");
            }
            return payload;
        }

        public static Payload GetOrDefault()
        {
            return RequestContext.Get("I") as Payload;
        }
    }
}
