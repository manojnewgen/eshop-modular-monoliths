using Shared.DDD;

namespace Catalog.Products.Events
{
    /// <summary>
    /// Domain event raised when a product is soft deleted
    /// </summary>
    /// <param name="ProductId">The ID of the deleted product</param>
    /// <param name="ProductName">The name of the deleted product</param>
    public record ProductDeletedEvent(Guid ProductId, string ProductName) : BaseDomainEvent;

    /// <summary>
    /// Domain event raised when a product is restored from soft delete
    /// </summary>
    /// <param name="ProductId">The ID of the restored product</param>
    /// <param name="ProductName">The name of the restored product</param>
    public record ProductRestoredEvent(Guid ProductId, string ProductName) : BaseDomainEvent;
}