using Orleans;
using System;

namespace OCore.Authorization
{

    [Serializable]
    [GenerateSerializer]
    public class AuthorizeOptions
    {
        [Id(0)]
        public string ApiKeyHeader { get; set; }
        [Id(1)]
        public string TokenHeader { get; set; }
        [Id(2)]
        public string TenantHeader { get; set; }
    }
}