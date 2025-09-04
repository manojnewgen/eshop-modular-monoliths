using MediatR;
using System;

namespace Shared.DDD
{
    public interface IDomainEvent : INotification
    {
        Guid EventId { get; }
        DateTime OccurredOn { get; }
        string EventType { get; }
    }

    public abstract record BaseDomainEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
        public string EventType => GetType().AssemblyQualifiedName!;
    }
}
