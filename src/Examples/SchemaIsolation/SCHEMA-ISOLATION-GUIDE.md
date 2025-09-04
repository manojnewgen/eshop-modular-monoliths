# ?? Schema-Based Data Isolation for Modular Monolith

## Overview
Jamie's shopping cart application uses **PostgreSQL schemas** to achieve complete data isolation between the CATALOG and BASKET modules while sharing the same database instance.

## ??? Architecture Implementation

### Database Schema Structure
```sql
-- Single PostgreSQL database: eshopdb
??? catalog schema
?   ??? products table
?   ??? categories table
?   ??? catalog-specific indexes
??? basket schema
?   ??? shopping_carts table
?   ??? cart_items table
?   ??? cart_discounts table
?   ??? basket-specific indexes
??? ordering schema (future)
??? shared schema
    ??? domain_events table
```

### Connection Configuration
```json
{
  "ConnectionStrings": {
    "CatalogConnection": "Host=eshop-db;Database=eshopdb;Schema=catalog;...",
    "BasketConnection": "Host=eshop-db;Database=eshopdb;Schema=basket;..."
  }
}
```

## ? How Schema Isolation Works

### 1. **Compile-Time Safety**
```csharp
// Catalog DbContext - ONLY accesses catalog schema
public class CatalogDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();
    // ? CANNOT access ShoppingCart - doesn't exist here!
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("catalog"); // All tables in catalog schema
    }
}

// Basket DbContext - ONLY accesses basket schema  
public class BasketDbContext : DbContext
{
    public DbSet<ShoppingCart> ShoppingCarts => Set<ShoppingCart>();
    // ? CANNOT access Product - doesn't exist here!
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("basket"); // All tables in basket schema
    }
}
```

### 2. **Runtime Database-Level Isolation**
```sql
-- Generated SQL for Catalog queries
SELECT * FROM catalog.products WHERE price > 100;

-- Generated SQL for Basket queries  
SELECT * FROM basket.shopping_carts WHERE status = 'Active';

-- ? Cross-schema access would require explicit schema qualification
-- This prevents accidental data access
```

### 3. **Loose Coupling Between Modules**
```csharp
// Adding product to cart - NO direct foreign key
public void AddProductToCart(Guid productId, string productName, decimal price)
{
    // Basket stores product reference + denormalized data
    cart.AddItem(
        productId: productId,        // Reference to catalog.products.id
        productName: productName,    // Denormalized for performance
        productPrice: price,         // Denormalized for performance
        quantity: 1,
        unitPrice: price
    );
    // ? No FK constraint across schemas = loose coupling
}
```

## ?? Immediate Benefits for CATALOG & BASKET Modules

### 1. **?? Data Security & Access Control**
- **Benefit**: Each module can only access its own data
- **Implementation**: Database users with schema-specific permissions
- **Result**: Zero risk of accidental cross-module data modification

### 2. **? Performance Optimization**
- **Benefit**: Schema-specific indexes and optimizations
- **Implementation**: Indexes tailored to each module's query patterns
- **Result**: Faster queries without index interference

### 3. **?? Independent Evolution**
- **Benefit**: Schema changes don't affect other modules
- **Implementation**: Module-specific migration scripts
- **Result**: Faster development cycles and safer deployments

### 4. **??? Clear Module Boundaries**
- **Benefit**: Enforced separation of concerns
- **Implementation**: Compile-time and runtime isolation
- **Result**: Better code organization and team autonomy

### 5. **?? Module-Specific Monitoring**
- **Benefit**: Granular database monitoring per module
- **Implementation**: Schema-level metrics and logging
- **Result**: Better troubleshooting and performance tuning

### 6. **?? Microservices Migration Path**
- **Benefit**: Easy extraction to separate services
- **Implementation**: Schema maps 1:1 to future microservice
- **Result**: Future-proof architecture

## ?? Practical Example

### Catalog Module Operation
```csharp
// Product management in catalog schema
await catalogContext.Products.AddAsync(new Product
{
    Name = "Gaming Laptop",
    Price = 1299.99m,
    Categories = new[] { "Electronics", "Gaming" }
});
await catalogContext.SaveChangesAsync();
// ? Data saved to catalog.products
```

### Basket Module Operation
```csharp
// Shopping cart management in basket schema
var cart = ShoppingCart.Create(userId);
cart.AddItem(
    productId: productFromCatalog.Id,  // Reference only
    productName: "Gaming Laptop",      // Denormalized
    productPrice: 1299.99m,           // Denormalized
    quantity: 1,
    unitPrice: 1299.99m
);
await basketContext.SaveChangesAsync();
// ? Data saved to basket.cart_items with denormalized product info
```

### Cross-Module Communication (Loose Coupling)
```csharp
// When product price changes in catalog
public async Task HandleProductPriceChanged(ProductPriceChangedEvent @event)
{
    // Update denormalized price in basket schema
    var cartItems = await basketContext.CartItems
        .Where(ci => ci.ProductId == @event.ProductId)
        .ToListAsync();
    
    foreach (var item in cartItems)
    {
        item.UpdatePrice(@event.NewPrice);
    }
    
    await basketContext.SaveChangesAsync();
    // ? Basket data updated independently through domain events
}
```

## ?? Key Schema Isolation Principles

1. **One Schema Per Module**: Each bounded context gets its own schema
2. **No Cross-Schema Foreign Keys**: Maintain loose coupling
3. **Denormalize When Needed**: Store necessary data locally for performance
4. **Use Domain Events**: Communicate changes between modules
5. **Schema-Specific Users**: Database users with limited schema access
6. **Independent Migrations**: Each module manages its own schema evolution

This approach gives Jamie the benefits of microservices (isolation, independence) while maintaining the simplicity of a monolithic deployment!