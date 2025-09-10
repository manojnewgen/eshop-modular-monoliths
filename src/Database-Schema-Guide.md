# ??? Database Schema Configuration Guide

## ?? **Schema Overview**

Your eShop application now uses the following PostgreSQL schemas for better organization:

| Schema | Purpose | Tables |
|--------|---------|---------|
| **`catalog`** | Product Catalog Module | Products, Categories, ProductCategories |
| **`basket`** | Shopping Cart Module | ShoppingCarts, ShoppingCartItems |
| **`ordering`** | Order Processing Module | Orders, OrderItems, Payments |
| **`identity`** | Keycloak Identity Server | Keycloak tables (auto-created) |
| **`shared`** | Shared/Audit Tables | AuditLogs, DomainEvents |
| **`messaging`** | Event/Message Store | IntegrationEvents, OutboxEvents |
| **`public`** | Default PostgreSQL Schema | System tables, extensions |

## ?? **Setup Instructions**

### **1. Run the Schema Setup Script**

```sql
-- Run this in your PostgreSQL as superuser (postgres)
psql -U postgres -d eshopdb -f database-schema-setup.sql
```

### **2. Update Your Entity Framework DbContext Classes**

For each module, update the `DbContext` to use the correct schema:

#### **Catalog Module DbContext**
```csharp
// In Modules/Catalog/Catalog/Data/CatalogDbContext.cs
public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Set default schema for all entities in this context
        modelBuilder.HasDefaultSchema("catalog");
        
        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        base.OnModelCreating(modelBuilder);
    }
}
```

#### **Basket Module DbContext**
```csharp
// In Modules/Basket/Basket/Data/BasketDbContext.cs
public class BasketDbContext : DbContext
{
    public BasketDbContext(DbContextOptions<BasketDbContext> options) : base(options) { }

    public DbSet<ShoppingCart> ShoppingCarts => Set<ShoppingCart>();
    public DbSet<ShoppingCartItem> ShoppingCartItems => Set<ShoppingCartItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Set default schema for all entities in this context
        modelBuilder.HasDefaultSchema("basket");
        
        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        base.OnModelCreating(modelBuilder);
    }
}
```

#### **Ordering Module DbContext**
```csharp
// In Modules/Ordering/Ordering/Data/OrderingDbContext.cs
public class OrderingDbContext : DbContext
{
    public OrderingDbContext(DbContextOptions<OrderingDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Set default schema for all entities in this context
        modelBuilder.HasDefaultSchema("ordering");
        
        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        base.OnModelCreating(modelBuilder);
    }
}
```

#### **Shared DbContext (for audit/events)**
```csharp
// In Shared/Shared/Data/SharedDbContext.cs
public class SharedDbContext : DbContext
{
    public SharedDbContext(DbContextOptions<SharedDbContext> options) : base(options) { }

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<DomainEventRecord> DomainEvents => Set<DomainEventRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Set default schema for shared entities
        modelBuilder.HasDefaultSchema("shared");
        
        base.OnModelCreating(modelBuilder);
    }
}
```

### **3. Update Entity Configuration Classes**

For entities that need to reference tables in different schemas:

```csharp
// Example: Product configuration that might reference shared audit table
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", "catalog");  // Explicit schema
        
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(200);
        
        // If referencing shared schema tables, use full schema.table name
        // builder.HasOne<AuditLog>().WithMany().HasForeignKey("AuditId").HasConstraintName("FK_Products_shared_AuditLogs");
    }
}
```

## ?? **Connection String Configuration**

Your connection strings are already configured with `SearchPath` parameter:

### **Local Development (docker-compose.local.yml)**
```
# Catalog module can access: catalog ? shared ? public (in that order)
CatalogConnection: "SearchPath=catalog,shared,public"

# Basket module can access: basket ? shared ? public
BasketConnection: "SearchPath=basket,shared,public"

# Ordering module can access: ordering ? shared ? public  
OrderingConnection: "SearchPath=ordering,shared,public"
```

## ?? **Migration Commands**

When creating migrations, specify the context and output directory:

```bash
# Catalog migrations
dotnet ef migrations add InitialCatalog --project Modules/Catalog/Catalog --startup-project Bootstrapper/Api --context CatalogDbContext --output-dir Data/Migrations

# Basket migrations
dotnet ef migrations add InitialBasket --project Modules/Basket/Basket --startup-project Bootstrapper/Api --context BasketDbContext --output-dir Data/Migrations

# Ordering migrations
dotnet ef migrations add InitialOrdering --project Modules/Ordering/Ordering --startup-project Bootstrapper/Api --context OrderingDbContext --output-dir Data/Migrations
```

## ?? **Querying Across Schemas**

### **Same Database, Different Schemas**
```sql
-- Query products from catalog schema
SELECT * FROM catalog.products;

-- Query shopping carts from basket schema
SELECT * FROM basket.shopping_carts;

-- Query across schemas (with explicit schema names)
SELECT 
    p.name,
    sci.quantity
FROM catalog.products p
JOIN basket.shopping_cart_items sci ON p.id = sci.product_id;
```

### **In Entity Framework (Cross-Schema Queries)**
```csharp
// This requires careful configuration and might need raw SQL for complex cross-schema queries
var query = dbContext.Database.SqlQueryRaw<ProductCartDto>(@"
    SELECT 
        p.name as ProductName,
        sci.quantity as Quantity
    FROM catalog.products p
    JOIN basket.shopping_cart_items sci ON p.id = sci.product_id
    WHERE sci.shopping_cart_id = {0}", cartId);
```

## ??? **Benefits of Schema Isolation**

### ? **Advantages:**
- **?? Security**: Each module has isolated data access
- **??? Organization**: Clear separation of concerns
- **?? Maintenance**: Easier to manage module-specific data
- **?? Reporting**: Clear data boundaries for analytics
- **?? Deployment**: Module-specific schema migrations
- **?? Team Development**: Different teams can work on different schemas

### **?? Schema-Specific Operations:**
- **Keycloak**: Automatically manages `identity` schema
- **Catalog**: Product, category, and inventory data in `catalog`
- **Basket**: Shopping cart data in `basket` schema
- **Ordering**: Order processing in `ordering` schema
- **Shared**: Audit logs, domain events in `shared` schema
- **Messaging**: Integration events, outbox in `messaging` schema

## ?? **Next Steps**

1. **Run the schema setup script** in your PostgreSQL
2. **Update your DbContext classes** to use the correct schemas
3. **Create migrations** for each module
4. **Test the schema isolation** by running your application
5. **Verify Keycloak** creates its tables in the `identity` schema

This setup provides you with a clean, organized database structure that scales well with your modular monolith architecture! ??