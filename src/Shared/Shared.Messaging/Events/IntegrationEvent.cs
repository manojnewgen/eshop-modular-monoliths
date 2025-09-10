using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Messaging.Events
{
    public record IntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();

        public DateTime CreationDate { get; init; } = DateTime.UtcNow;

        public string EventType => GetType().AssemblyQualifiedName ?? string.Empty;
    }
}
