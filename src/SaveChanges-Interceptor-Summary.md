# SaveChanges Interceptor with Domain Event Dispatching

## Overview

A clean, production-ready SaveChanges interceptor that automatically handles:
- **Audit fields** (CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy)
- **Soft deletes** (converts hard deletes to updates with IsDeleted = true)
- **Domain event dispatching** through MediatR after successful saves

## Key Features

### 1. Automatic Audit Fields
```csharp
// Automatically sets on entity creation
entity.CreatedAt = DateTime.UtcNow;
entity.CreatedBy = currentUser;

// Automatically sets on entity modification
entity.LastModifiedAt = DateTime.UtcNow;
entity.LastModifiedBy = currentUser;
```

### 2. Soft Delete Conversion
```csharp
// When you call: context.Products.Remove(product);
// Interceptor converts to:
product.IsDeleted = true;
product.DeletedAt = DateTime.UtcNow;
product.DeletedBy = currentUser;
```

### 3. Domain Event Dispatching
```csharp
// Events are collected during SaveChanges and dispatched after success
public void UpdatePrice(decimal newPrice)
{
    Price = newPrice;
    AddDomainEvent(new ProductPriceChangedEvent(Id, oldPrice, newPrice));
}
// Event is automatically dispatched through MediatR after SaveChanges
```

## Setup

### 1. Register in Module
```csharp
public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
{
    services.AddSaveChangesInterceptor();
    
    services.AddDbContext<CatalogDbContext>((serviceProvider, options) =>
    {
        options.UseNpgsql(connectionString);
        options.AddSaveChangesInterceptor(serviceProvider);
    });
    
    // Register MediatR for domain events
    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
    
    return services;
}
```

### 2. Entity Configuration
```csharp
public class Product : Aggregate<Guid>
{
    // Audit fields inherited from Entity<T>
    // public DateTime? CreatedAt { get; set; }
    // public string CreatedBy { get; set; }
    // public DateTime? LastModifiedAt { get; set; }
    // public string LastModifiedBy { get; set; }
    
    // Soft delete fields
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }
}
```

### 3. EF Configuration
```csharp
builder.Property(p => p.IsDeleted)
    .HasColumnName("is_deleted")
    .IsRequired()
    .HasDefaultValue(false);

// Exclude soft-deleted items from queries
builder.HasQueryFilter(p => !p.IsDeleted);

// Ignore domain events in EF mapping
builder.Ignore(p => p.DomainEvents);
builder.Ignore(p => p.Events);
```

### 4. Domain Event Handlers
```csharp
public class ProductEventHandlers : INotificationHandler<ProductCreatedEvent>
{
    public Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Handle the event (logging, notifications, etc.)
        return Task.CompletedTask;
    }
}
```

## Usage Example

```csharp
// Create product - audit fields set automatically
var product = Product.Create(id, "New Product", "Description", 99.99m, "image.jpg");
context.Products.Add(product);
await context.SaveChangesAsync(); // ProductCreatedEvent dispatched

// Update product - audit fields set automatically  
product.UpdatePrice(149.99m); // Raises ProductPriceChangedEvent
await context.SaveChangesAsync(); // Event dispatched

// Soft delete - converted from hard delete
context.Products.Remove(product); // Raises ProductDeletedEvent
await context.SaveChangesAsync(); // Soft delete applied, event dispatched

// Query excludes soft-deleted by default
var activeProducts = context.Products.ToList(); // Soft-deleted items excluded
var allProducts = context.Products.IgnoreQueryFilters().ToList(); // Include all
```

## Benefits

- **Zero boilerplate**: Audit fields and soft deletes happen automatically
- **Event-driven**: Domain events are reliably dispatched after successful saves
- **Consistent**: Same behavior across all modules
- **Clean code**: No repetitive audit/event handling code in business logic
- **Reliable**: Events only dispatch after successful database saves

This interceptor provides enterprise-grade cross-cutting concerns in a clean, maintainable way.