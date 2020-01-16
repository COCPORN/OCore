using Orleans;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using OCore.Authorization.Abstractions.Request;

namespace OCore.Entities
{
    public class Entity<T>: Grain<EntityState<T>>, IEntity
        where T: new()
    {
        EntityLogic<T> entityLogic;

        ILogger Logger => (ILogger<Entity<T>>)ServiceProvider.GetService(typeof(ILogger<Entity<T>>));

        public override async Task OnActivateAsync()
        {
            entityLogic = new EntityLogic<T>(base.State,
                base.WriteStateAsync,
                OnCreating,
                Upgrade,
                this,
                Logger,
                Version);
            
            if (base.State.Created == false)
            {
                if (GetType().GetCustomAttributes(typeof(SuppressIndexingAttribute), true).Length == 0)
                {
                    var requestPayload = Payload.GetOrDefault();

                    if (requestPayload != null)
                    {
                        base.State.TenantId = requestPayload.TenantId;
                    }

                    if (this is IGrainWithStringKey)
                    {
                        var primaryKeyString = this.GetPrimaryKeyString();

                        if (base.State.TenantId != null)
                        {
                            var suffix = $"::{base.State.TenantId}";
                            if (primaryKeyString.EndsWith(suffix))
                            {
                                primaryKeyString = primaryKeyString.Substring(0, primaryKeyString.Length - suffix.Length);
                            }
                        }

                        base.State.KeyString = primaryKeyString;
                    }
                    else if (this is IGrainWithGuidCompoundKey)
                    {
                        base.State.KeyGuid = this.GetPrimaryKey(out var keyExtension);
                        base.State.KeyExtension = keyExtension;
                    }
                    else if (this is IGrainWithGuidKey)
                    {
                        base.State.KeyGuid = this.GetPrimaryKey();
                    }
                    else if (this is IGrainWithIntegerKey)
                    {
                        base.State.KeyLong = this.GetPrimaryKeyLong();
                    }
                    else if (this is IGrainWithIntegerCompoundKey)
                    {
                        base.State.KeyLong = this.GetPrimaryKeyLong(out var keyExtension);
                        base.State.KeyExtension = keyExtension;
                    }
                }
            }

            await entityLogic.OnActivateAsync();
            activated = true;
            await base.OnActivateAsync();
        }

        bool activated;

        protected DateTimeOffset CreatedAt => base.State.CreatedAt;

        protected bool Created => base.State.Created;

        protected DateTimeOffset UpdatedAt => base.State.UpdatedAt;

        protected int Version { get; set; }

        void EnsureEntityActivated()
        {
            if (activated == false)
            {
                throw new InvalidOperationException("Make sure the entity has been activated before trying to access keys. Did you forget to call base.OnActivateAsync()?");
            }
        }

        public string PrimaryKeyString
        {
            get
            {
                EnsureEntityActivated();
                return base.State.KeyString;
            }
        }

        public Guid PrimaryKeyGuid
        {
            get
            {
                EnsureEntityActivated();
                return base.State.KeyGuid.Value;
            }
        }
        public long PrimaryKeyLong
        {
            get
            {
                EnsureEntityActivated();
                return base.State.KeyLong.Value;
            }
        }
        public string KeyExtension
        {
            get
            {
                EnsureEntityActivated();
                return base.State.KeyExtension;
            }
        }

        protected virtual Task<int> Upgrade(int from)
        {
            return Task.FromResult(0);
        }

        protected Task Delete()
        {
            return entityLogic.Delete();
        }

        protected new T State
        {
            get
            {
                EntityState<T> s = base.State;
                if (s.Data == null)
                {
                    s.Data = new T();
                }
                return s.Data;
            }

            set
            {
                EntityState<T> s = base.State;
                s.Data = value;
            }
        }

        protected virtual Task OnCreating()
        {
            return Task.CompletedTask;
        }

        protected override Task WriteStateAsync()
        {
            return entityLogic.WriteStateAsync();
        }

        Task IEntity.ReadStateAsync()
        {
            return base.ReadStateAsync();
        }
    }
}
