using System;

namespace IO.Astrodynamics.Models.SeedWork
{
    public interface IEntity
    {
        public int Id { get; }
        public Guid? TenantId { get; }
        public int GroupId { get; }

        public void SetTenantId(Guid tenant);
    }
}