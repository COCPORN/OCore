using Orleans;
using System;

namespace OCore.Authorization
{

    [Serializable]
    [GenerateSerializer]
    public class AuthorizeOptions
    {
        public string ApiKeyHeader { get; set; }
        public string TokenHeader { get; set; }
        public string TenantHeader { get; set; }
    }
}