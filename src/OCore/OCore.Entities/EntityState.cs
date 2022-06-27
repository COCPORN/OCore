using OCore.Core;
using Orleans;
using System;

namespace OCore.Entities
{
    [Serializable]
    [GenerateSerializer]
    public class EntityState<T>
    {
        public bool Created { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        public int Version { get; set; }

        public Key Key { get; set; }

        public string TenantId { get; set; }

        public T Data;
    }
}
