using Shared.DDD;

namespace Catalog.Products.Events
{
    public record ProductCreatedEvent(
        Guid ProductId,
        string Name,
        decimal Price,
        List<string> Categories) : BaseDomainEvent;
}