using Shared.DDD;

namespace Basket.ShoppingCarts.Events
{
    public record ShoppingCartCreatedEvent(
        Guid CartId,
        Guid UserId,
        string? SessionId) : BaseDomainEvent;

    public record CartItemAddedEvent(
        Guid CartId,
        Guid ProductId,
        string ProductName,
        int Quantity,
        decimal UnitPrice) : BaseDomainEvent;

    public record CartItemRemovedEvent(
        Guid CartId,
        Guid ProductId,
        string ProductName) : BaseDomainEvent;

    public record CartItemQuantityUpdatedEvent(
        Guid CartId,
        Guid ProductId,
        int NewQuantity) : BaseDomainEvent;

    public record DiscountAppliedEvent(
        Guid CartId,
        string DiscountCode,
        string DiscountType,
        decimal DiscountValue) : BaseDomainEvent;

    public record ShoppingCartClearedEvent(
        Guid CartId,
        Guid UserId) : BaseDomainEvent;

    public record ShoppingCartCheckedOutEvent(
        Guid CartId,
        Guid UserId,
        decimal Total,
        int ItemCount) : BaseDomainEvent;
}