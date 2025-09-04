# ?? SaveChanges Interceptor - Complete Guide

## ?? Overview

The SaveChanges interceptor is a powerful Entity Framework Core feature that automatically handles common cross-cutting concerns when saving data to the database. This implementation provides:

- ? **Automatic audit fields** (CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy)
- ? **Soft delete functionality** (IsDeleted, DeletedAt, DeletedBy)
- ? **Domain events collection** and logging
- ? **Performance monitoring** and detailed logging
- ? **Change tracking** for debugging purposes

## ?? Quick Start

### 1. Register the Interceptor in Your Module

The easiest way is to use the `AddDbContextWithInterceptor` extension method:

```csharp
public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("CatalogConnection");
    
    // ?? This automatically registers the SaveChanges interceptor
    services.AddDbContextWithInterceptor<CatalogDbContext>(
        connectionString: connectionString,
        schemaName: "catalog"
    );
    
    return services;
}
```

### 2. Add Audit Fields to Your Entities

Add these properties to any entity you want to track:

```csharp
public class Product : Aggregate<Guid>
{
    // Your business properties...
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    
    // ?? Audit fields (automatically managed by interceptor)
    public DateTime CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }
    
    // ?? Soft delete fields (automatically managed by interceptor)
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }
}
```

### 3. Configure Entity Mapping

Update your Entity Framework configuration:

```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // Your existing configuration...
        
        // ?? Audit fields
        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
            
        builder.Property(p => p.CreatedBy)
            .HasMaxLength(100)
            .HasColumnName("created_by");
            
        builder.Property(p => p.LastModifiedAt)
            .HasColumnName("last_modified_at");
            
        builder.Property(p => p.LastModifiedBy)
            .HasMaxLength(100)
            .HasColumnName("last_modified_by");

        // ?? Soft delete fields
        builder.Property(p => p.IsDeleted)
            .HasColumnName("is_deleted")
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(p => p.DeletedAt)
            .HasColumnName("deleted_at");
            
        builder.Property(p => p.DeletedBy)
            .HasMaxLength(100)
            .HasColumnName("deleted_by");

        // ?? Query filter to exclude soft-deleted items
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
```

## ?? Features in Detail

### ?? Automatic Audit Fields

The interceptor automatically sets these fields:

| Field | When Set | Description |
|-------|----------|-------------|
| `CreatedAt` | Entity creation | UTC timestamp when entity was first saved |
| `CreatedBy` | Entity creation | User who created the entity |
| `LastModifiedAt` | Entity update | UTC timestamp when entity was last modified |
| `LastModifiedBy` | Entity update | User who last modified the entity |

```csharp
// Create a new product
var product = Product.Create(id, "New Product", "Description", 99.99m, "image.jpg");
context.Products.Add(product);

await context.SaveChangesAsync();
// ? CreatedAt and CreatedBy are automatically set

// Update the product
product.UpdatePrice(149.99m);
await context.SaveChangesAsync();
// ? LastModifiedAt and LastModifiedBy are automatically set
```

### ??? Soft Delete Functionality

Instead of permanently deleting records, the interceptor converts delete operations to updates:

```csharp
// This looks like a delete...
var product = context.Products.Find(productId);
context.Products.Remove(product);

await context.SaveChangesAsync();
// ? But the interceptor converts it to an update:
// - Sets IsDeleted = true
// - Sets DeletedAt = current timestamp
// - Sets DeletedBy = current user
// - Entity remains in database
```

**Query Behavior:**
```csharp
// Normal queries automatically exclude soft-deleted items
var products = context.Products.ToList(); // Only active products

// Include soft-deleted items explicitly
var allProducts = context.Products.IgnoreQueryFilters().ToList(); // All products
```

### ?? Domain Events Integration

The interceptor automatically collects and logs domain events:

```csharp
public class Product : Aggregate<Guid>
{
    public void UpdatePrice(decimal newPrice, string reason)
    {
        var oldPrice = Price;
        Price = newPrice;
        
        // ?? Raise domain event
        AddDomainEvent(new ProductPriceChangedEvent(Id, oldPrice, newPrice, reason));
    }
}

// When you save changes...
await context.SaveChangesAsync();
// ? The interceptor logs all domain events and can publish them
```

### ?? Performance Monitoring

The interceptor provides detailed logging:

```
[10:30:00 INF] Processing 3 entity changes in CatalogDbContext
[10:30:00 DBG] Entity changes being saved: Added Product(123), Modified Product(456): Name, Price, Deleted Product(789)
[10:30:00 DBG] Found 2 domain events on Product: ProductPriceChangedEvent, ProductCategoriesUpdatedEvent
[10:30:01 INF] SaveChanges completed successfully for CatalogDbContext. 3 records affected
```

## ?? Advanced Configuration

### Manual Registration

If you need more control over the interceptor registration:

```csharp
public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
{
    // Register the interceptor services
    services.AddSaveChangesInterceptor();
    
    services.AddDbContext<CatalogDbContext>((serviceProvider, options) =>
    {
        var connectionString = configuration.GetConnectionString("CatalogConnection");
        
        options.UseNpgsql(connectionString, npgsql =>
        {
            npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "catalog");
            npgsql.EnableRetryOnFailure(maxRetryCount: 3);
        });

        // Add the SaveChanges interceptor
        options.AddSaveChangesInterceptor(serviceProvider);

        // Development settings
        var environment = serviceProvider.GetService<IHostEnvironment>();
        if (environment?.IsDevelopment() == true)
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }
    });
    
    return services;
}
```

### Custom Interceptor

You can create a custom interceptor for module-specific logic:

```csharp
public class CatalogSaveChangesInterceptor : SaveChangesInterceptor
{
    public CatalogSaveChangesInterceptor(ILogger<CatalogSaveChangesInterceptor> logger) 
        : base(logger) { }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, 
        InterceptionResult<int> result)
    {
        // Custom logic specific to Catalog module
        if (eventData.Context is CatalogDbContext catalogContext)
        {
            // Handle catalog-specific business rules
            ValidateProductPrices(catalogContext);
        }

        return base.SavingChanges(eventData, result);
    }

    private void ValidateProductPrices(CatalogDbContext context)
    {
        var products = context.ChangeTracker.Entries<Product>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .Select(e => e.Entity);

        foreach (var product in products)
        {
            if (product.Price <= 0)
            {
                throw new InvalidOperationException($"Product {product.Name} has invalid price: {product.Price}");
            }
        }
    }
}
```

## ?? Database Migration

Create a migration to add the audit fields to existing tables:

```bash
# Add migration for audit fields
dotnet ef migrations add AddAuditFields --project Modules/Catalog/Catalog

# Update database
dotnet ef database update --project Modules/Catalog/Catalog
```

The migration will add:
```sql
ALTER TABLE catalog.products 
ADD COLUMN created_at TIMESTAMP NOT NULL DEFAULT NOW(),
ADD COLUMN created_by VARCHAR(100),
ADD COLUMN last_modified_at TIMESTAMP,
ADD COLUMN last_modified_by VARCHAR(100),
ADD COLUMN is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
ADD COLUMN deleted_at TIMESTAMP,
ADD COLUMN deleted_by VARCHAR(100);

CREATE INDEX idx_products_is_deleted ON catalog.products (is_deleted);
CREATE INDEX idx_products_created_at ON catalog.products (created_at);
```

## ?? Testing

### Example Usage

```csharp
public async Task ExampleUsage()
{
    using var scope = app.ApplicationServices.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

    // 1. Create - audit fields set automatically
    var product = Product.Create(Guid.NewGuid(), "Test Product", "Description", 99.99m, "test.jpg");
    context.Products.Add(product);
    await context.SaveChangesAsync();
    
    Console.WriteLine($"Created: {product.CreatedAt} by {product.CreatedBy}");

    // 2. Update - modification fields set automatically
    product.UpdatePrice(149.99m);
    await context.SaveChangesAsync();
    
    Console.WriteLine($"Modified: {product.LastModifiedAt} by {product.LastModifiedBy}");

    // 3. Soft delete - delete fields set automatically
    context.Products.Remove(product);
    await context.SaveChangesAsync();
    
    Console.WriteLine($"Deleted: {product.DeletedAt} by {product.DeletedBy}");
    Console.WriteLine($"Is deleted: {product.IsDeleted}");

    // 4. Query excludes soft-deleted by default
    var activeProducts = context.Products.ToList(); // Doesn't include deleted product
    var allProducts = context.Products.IgnoreQueryFilters().ToList(); // Includes deleted product
}
```

## ?? Troubleshooting

### Common Issues

1. **Audit fields not being set**
   - Ensure the interceptor is registered correctly
   - Check that property names match exactly (case-sensitive)
   - Verify the entity has the properties defined

2. **Soft delete not working**
   - Ensure `IsDeleted` property exists on the entity
   - Check that the query filter is configured
   - Verify the property is mapped correctly in EF configuration

3. **Performance issues**
   - The interceptor processes all tracked entities on SaveChanges
   - Consider disabling debug logging in production
   - Use bulk operations for large datasets

### Debug Logging

Enable debug logging to see interceptor activity:

```json
{
  "Logging": {
    "LogLevel": {
      "Shared.Data.Interceptors.SaveChangesInterceptor": "Debug"
    }
  }
}
```

## ?? Benefits

Using the SaveChanges interceptor provides:

- ? **Consistency**: All entities get the same audit behavior
- ? **Automatic**: No need to remember to set audit fields manually
- ? **Safe deletes**: Soft delete prevents accidental data loss
- ? **Observability**: Detailed logging of all data changes
- ? **Domain events**: Automatic collection and logging
- ? **Maintainable**: Centralized cross-cutting concern handling

This interceptor is a powerful tool for implementing enterprise-grade audit trails and soft delete functionality in your modular monolith! ??