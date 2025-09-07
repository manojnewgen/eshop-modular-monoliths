
namespace Basket.Basket.DTOs
{
    public record ShoppingCartItemDto(
       Guid ShoppingCartId,
       Guid ProductId,
       int Quantity,
       decimal Price,
       string ProductName,
       string Color
    );
}
