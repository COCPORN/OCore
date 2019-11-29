using Orleans.Runtime;
using System;
using System.Collections.Generic;

namespace OCore.Authorization.Abstractions.Request
{
    public class Payload
    {
        public Guid Token { get; set; }

        public Guid ApiKey { get; set; }

        public List<string> ApiKeyApplications { get; set; }

        public Guid? AccountId { get; set; }

        public bool AccountIdHasBeenProjected { get; set; }

        public string TenantId { get; set; }

        public bool IsRequestElevated { get; set; }

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
