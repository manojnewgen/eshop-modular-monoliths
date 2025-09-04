using Shared.DDD;
using Basket.ShoppingCarts.Events;

namespace Basket.ShoppingCarts
{
    /// <summary>
    /// Shopping Cart Aggregate Root - Lives in 'basket' schema
    /// </summary>
    public class ShoppingCart : Aggregate<Guid>
    {
        private readonly List<CartItem> _items = new();
        private readonly List<CartDiscount> _discounts = new();

        public Guid UserId { get; private set; }
        public string? SessionId { get; private set; }
        public string Status { get; private set; } = "Active";
        public new DateTime CreatedAt { get; private set; }
        public new DateTime LastModifiedAt { get; private set; }

        // Navigation properties
        public IReadOnlyList<CartItem> Items => _items.AsReadOnly();
        public IReadOnlyList<CartDiscount> Discounts => _discounts.AsReadOnly();

        // Computed properties
        public decimal SubTotal => _items.Sum(i => i.TotalPrice);
        public decimal DiscountAmount => _discounts.Sum(d => d.CalculateDiscount(SubTotal));
        public decimal Total => SubTotal - DiscountAmount;
        public int ItemCount => _items.Sum(i => i.Quantity);

        // Required for EF Core
        private ShoppingCart() { }

        // Private constructor
        private ShoppingCart(Guid id, Guid userId, string? sessionId) : base(id)
        {
            UserId = userId;
            SessionId = sessionId;
            CreatedAt = DateTime.UtcNow;
            LastModifiedAt = DateTime.UtcNow;
        }

        // Factory method
        public static ShoppingCart Create(Guid userId, string? sessionId = null)
        {
            var cart = new ShoppingCart(Guid.NewGuid(), userId, sessionId);
            
            // Raise domain event
            cart.AddDomainEvent(new ShoppingCartCreatedEvent(cart.Id, cart.UserId, cart.SessionId));
            
            return cart;
        }

        // Business methods
        public void AddItem(Guid productId, string productName, decimal productPrice, string? productImageUrl, int quantity, decimal unitPrice)
        {
            ValidateActiveCart();
            
            var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
            
            if (existingItem != null)
            {
                existingItem.UpdateQuantity(existingItem.Quantity + quantity);
                AddDomainEvent(new CartItemQuantityUpdatedEvent(Id, productId, existingItem.Quantity));
            }
            else
            {
                var newItem = CartItem.Create(Id, productId, productName, productPrice, productImageUrl, quantity, unitPrice);
                _items.Add(newItem);
                AddDomainEvent(new CartItemAddedEvent(Id, productId, productName, quantity, unitPrice));
            }
            
            UpdateLastModified();
        }

        public void RemoveItem(Guid productId)
        {
            ValidateActiveCart();
            
            var item = _items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                _items.Remove(item);
                AddDomainEvent(new CartItemRemovedEvent(Id, productId, item.ProductName));
                UpdateLastModified();
            }
        }

        public void UpdateItemQuantity(Guid productId, int quantity)
        {
            ValidateActiveCart();
            
            var item = _items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                if (quantity <= 0)
                {
                    RemoveItem(productId);
                }
                else
                {
                    item.UpdateQuantity(quantity);
                    AddDomainEvent(new CartItemQuantityUpdatedEvent(Id, productId, quantity));
                    UpdateLastModified();
                }
            }
        }

        public void Clear()
        {
            ValidateActiveCart();
            
            _items.Clear();
            _discounts.Clear();
            AddDomainEvent(new ShoppingCartClearedEvent(Id, UserId));
            UpdateLastModified();
        }

        public void ApplyDiscount(string discountCode, string discountType, decimal discountValue)
        {
            ValidateActiveCart();
            
            // Remove existing discount with same code
            _discounts.RemoveAll(d => d.DiscountCode == discountCode);
            
            var discount = CartDiscount.Create(Id, discountCode, discountType, discountValue);
            _discounts.Add(discount);
            
            AddDomainEvent(new DiscountAppliedEvent(Id, discountCode, discountType, discountValue));
            UpdateLastModified();
        }

        public void Checkout()
        {
            ValidateActiveCart();
            
            if (!_items.Any())
                throw new InvalidOperationException("Cannot checkout empty cart");
            
            Status = "CheckedOut";
            AddDomainEvent(new ShoppingCartCheckedOutEvent(Id, UserId, Total, ItemCount));
            UpdateLastModified();
        }

        private void ValidateActiveCart()
        {
            if (Status != "Active")
                throw new InvalidOperationException($"Cannot modify cart with status: {Status}");
        }

        private void UpdateLastModified()
        {
            LastModifiedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Cart Item Entity - Lives in 'basket' schema
    /// </summary>
    public class CartItem : Entity<Guid>
    {
        public Guid CartId { get; private set; }
        public Guid ProductId { get; private set; } // References catalog.products loosely
        public string ProductName { get; private set; } = default!; // Denormalized
        public decimal ProductPrice { get; private set; } // Denormalized
        public string? ProductImageUrl { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal TotalPrice => Quantity * UnitPrice;
        public DateTime AddedAt { get; private set; }
        public new DateTime LastModifiedAt { get; private set; }

        // Navigation property
        public ShoppingCart ShoppingCart { get; private set; } = default!;

        // Required for EF Core
        private CartItem() { }

        private CartItem(Guid id, Guid cartId, Guid productId, string productName, decimal productPrice, 
            string? productImageUrl, int quantity, decimal unitPrice) : base(id)
        {
            CartId = cartId;
            ProductId = productId;
            ProductName = productName;
            ProductPrice = productPrice;
            ProductImageUrl = productImageUrl;
            Quantity = quantity;
            UnitPrice = unitPrice;
            AddedAt = DateTime.UtcNow;
            LastModifiedAt = DateTime.UtcNow;
        }

        public static CartItem Create(Guid cartId, Guid productId, string productName, decimal productPrice,
            string? productImageUrl, int quantity, decimal unitPrice)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero");
            
            if (unitPrice < 0)
                throw new ArgumentException("Unit price cannot be negative");

            return new CartItem(Guid.NewGuid(), cartId, productId, productName, productPrice, 
                productImageUrl, quantity, unitPrice);
        }

        public void UpdateQuantity(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero");
            
            Quantity = quantity;
            LastModifiedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Cart Discount Entity - Lives in 'basket' schema
    /// </summary>
    public class CartDiscount : Entity<Guid>
    {
        public Guid CartId { get; private set; }
        public string DiscountCode { get; private set; } = default!;
        public string DiscountType { get; private set; } = default!; // PERCENTAGE or FIXED
        public decimal DiscountValue { get; private set; }
        public DateTime AppliedAt { get; private set; }

        // Navigation property
        public ShoppingCart ShoppingCart { get; private set; } = default!;

        // Required for EF Core
        private CartDiscount() { }

        private CartDiscount(Guid id, Guid cartId, string discountCode, string discountType, decimal discountValue) : base(id)
        {
            CartId = cartId;
            DiscountCode = discountCode;
            DiscountType = discountType;
            DiscountValue = discountValue;
            AppliedAt = DateTime.UtcNow;
        }

        public static CartDiscount Create(Guid cartId, string discountCode, string discountType, decimal discountValue)
        {
            if (string.IsNullOrWhiteSpace(discountCode))
                throw new ArgumentException("Discount code cannot be empty");
            
            if (!new[] { "PERCENTAGE", "FIXED" }.Contains(discountType))
                throw new ArgumentException("Discount type must be PERCENTAGE or FIXED");
            
            if (discountValue < 0)
                throw new ArgumentException("Discount value cannot be negative");

            return new CartDiscount(Guid.NewGuid(), cartId, discountCode, discountType, discountValue);
        }

        public decimal CalculateDiscount(decimal subtotal)
        {
            return DiscountType switch
            {
                "PERCENTAGE" => Math.Min(subtotal * (DiscountValue / 100), subtotal),
                "FIXED" => Math.Min(DiscountValue, subtotal),
                _ => 0
            };
        }
    }
}