using Shared.DDD;

namespace Catalog.Products.Events
{
    public record ProductCategoriesUpdatedEvent(
        Guid ProductId,
        List<string> OldCategories,
        List<string> NewCategories) : BaseDomainEvent;
}