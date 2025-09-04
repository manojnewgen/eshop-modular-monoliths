using Shared.DDD;

namespace Catalog.Products.Events
{
    public record ProductPriceChangedEvent(
        Guid ProductId,
        decimal OldPrice,
        decimal NewPrice,
        string Reason) : BaseDomainEvent;
}