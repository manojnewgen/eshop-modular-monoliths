using System.Text.Json.Serialization;

namespace Basket.Basket.Modules
{
    public class ShoppingCartItem : Entity<Guid>
    {
        public Guid ShoppingCartId { get; private set; } = default;

        public Guid ProductId { get; private set; } = default;

        public int Quantity { get; internal set; } = default;

        public decimal Price { get; private set; } = default;

        public string ProductName { get; private set; } = default!;

        public string Color { get; private set; } = default!;

        internal ShoppingCartItem(Guid shoppingCartId, Guid productId, int quantity, decimal price, string color)
        {
            ProductId = productId;
            Id = Guid.NewGuid();
            Quantity = quantity;
            Price = price;
            ProductName = string.Empty;
            Color = color;
            ShoppingCartId = shoppingCartId;
        }

        [JsonConstructor]
        public ShoppingCartItem(Guid shoppingCartId, Guid productId, int quantity, decimal price, string productName, string color)
        {
            ProductId = productId;
            Id = Guid.NewGuid();
            Quantity = quantity;
            Price = price;
            ProductName = productName;
            Color = color;
            ShoppingCartId = shoppingCartId;
        }

        public void UpdatePrice(decimal newPrice)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(newPrice);
            Price = newPrice;
        }
    }
}
