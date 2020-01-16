using System;
using System.Collections.Generic;
using System.Text;

namespace OCore.Entities
{
    public class EntityState<T>
    {
        public bool Created { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public int Version { get; set; }

        public long? KeyLong { get; set; }

        public string KeyString { get; set; }

        public Guid? KeyGuid { get; set; }

        public string KeyExtension { get; set; }

        public string TenantId { get; set; }

        public T Data;
    }
}
