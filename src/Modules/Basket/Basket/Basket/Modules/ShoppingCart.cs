namespace Basket.Basket.Modules
{
    public class ShoppingCart: Aggregate<Guid>
    {
        public string UserName { get; private set; } = default!;

        private readonly List<ShoppingCartItem> _items = new List<ShoppingCartItem>();

        public IReadOnlyList<ShoppingCartItem> Items => _items.AsReadOnly();
        public decimal TotalPrice => _items.Sum(i => i.Price * i.Quantity);

        public static ShoppingCart Create(Guid id, string userName)
        {
            var cart = new ShoppingCart
            {
                Id = id,
                UserName = userName
            };
            //cart.AddDomainEvent(new ShoppingCartCreatedEvent(cart));
            return cart;
        }

        public void AddItem(Guid productId, int quantity, string color, decimal price, string productName)
        {
            var existingItem = _items.FirstOrDefault(i => i.ProductId == productId && i.Color == color);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                // Fixed: Use the correct constructor with all required parameters
                var newItem = new ShoppingCartItem(this.Id, productId, quantity, price, productName, color);
                _items.Add(newItem);
            }
            //AddDomainEvent(new ShoppingCartItemAddedEvent(this, productId, quantity));
        }

        public void RemoveItem(Guid productId, string color)
        {
            var item = _items.FirstOrDefault(i => i.ProductId == productId && i.Color == color);
            if (item != null)
            {
                _items.Remove(item);
                //AddDomainEvent(new ShoppingCartItemRemovedEvent(this, productId));
            }
        }
    }
}
