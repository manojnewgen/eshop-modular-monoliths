using Shared.DDD;

namespace Catalog.Products.Events
{
    public record ProductPriceChangedEvent(
        Guid ProductId,
        decimal OldPrice,
        decimal NewPrice,
        Product Product,
        string Reason) : BaseDomainEvent;
}