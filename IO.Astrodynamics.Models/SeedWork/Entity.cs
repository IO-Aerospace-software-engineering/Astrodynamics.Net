using System;
using System.Collections.Generic;

namespace IO.Astrodynamics.Models.SeedWork
{
    public class Entity : IEntity, IEquatable<Entity>
    {
        int _Id;

        public Entity(int id = default)
        {
            Id = id;
        }

        public virtual int Id
        {
            get { return _Id; }
            protected set { _Id = value; }
        }

        public Guid? TenantId { get; private set; }
        public int GroupId { get; protected set; }

        public void SetTenantId(Guid tenantId)
        {
            if (TenantId.HasValue && TenantId != default && TenantId != tenantId)
            {
                throw new InvalidOperationException("Tenant id can be only set on orphan entity ");
            }

            TenantId = tenantId;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Entity);
        }

        public bool Equals(Entity other)
        {
            return other is not null && ReferenceEquals(this, other) ||
                   (!IsTransient()
                    && !other.IsTransient()
                    && _Id == other._Id);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_Id);
        }

        public bool IsTransient()
        {
            return this.Id == default;
        }

        public static bool operator ==(Entity left, Entity right)
        {
            return EqualityComparer<Entity>.Default.Equals(left, right);
        }

        public static bool operator !=(Entity left, Entity right)
        {
            return !(left == right);
        }
    }
}