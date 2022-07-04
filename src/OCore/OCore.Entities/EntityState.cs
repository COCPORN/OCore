using OCore.Core;
using Orleans;
using System;

namespace OCore.Entities
{
    [Serializable]
    [GenerateSerializer]
    public class EntityState<T>
    {
        [Id(0)]
        public bool Created { get; set; }

        [Id(1)]
        public DateTimeOffset CreatedAt { get; set; }
        [Id(2)]
        public DateTimeOffset UpdatedAt { get; set; }

        [Id(3)]
        public int Version { get; set; }

        [Id(4)]
        public Key Key { get; set; }

        [Id(5)]
        public string TenantId { get; set; }

        [Id(6)]
        public T Data;
    }
}
