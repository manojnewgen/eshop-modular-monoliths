using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DDD
{
    public abstract class Entity<T> : IEntity<T>
    {
        public T Id { get; protected set; } = default!;

        public DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; } = default!;
        public DateTime? LastModifiedAt { get; set; }
        public string LastModifiedBy { get; set; } = default!;

        protected Entity() { }

        protected Entity(T id)
        {
            Id = id;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Entity<T> other)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (Id == null || other.Id == null)
                return false;

            return Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id?.GetHashCode() ?? 0;
        }

        public static bool operator ==(Entity<T>? left, Entity<T>? right)
        {
            if (left is null && right is null)
                return true;

            if (left is null || right is null)
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(Entity<T>? left, Entity<T>? right)
        {
            return !(left == right);
        }
    }
}
